using System.Net;
using Serilog;

namespace Chaos;

public sealed class KcpService : Service
{
    public const int ConnectTimeout = 20 * 1000;

    private readonly long creationTime;

    // 当前时间 - KService创建的时间, 线程安全
    public uint TimeNow => (uint)(TimeInfo.Default.ClientNow() - this.creationTime);

    public IKcpTransport Transport;

    public NetworkProtocol Protocol { get; set; }

    public KcpService(IKcpTransport kcpTransport, ServiceType serviceType)
    {
        this.ServiceType = serviceType;
        this.creationTime = TimeInfo.Default.ClientNow();
        this.Transport = kcpTransport;
    }

    private readonly Dictionary<long, KcpChannel> localConnectionChannels = new();
    private readonly Dictionary<long, KcpChannel> waitAcceptChannels = new();

    private readonly byte[] cache = new byte[2048];

    private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

    private readonly List<long> cacheIds = new();

    private readonly HashSet<long> updateIds = new();

    // 下次时间更新的channel
    private readonly SortedListDictionary<long, long> timeId = new(1000);

    private readonly List<long> timeOutTime = new();

    // 记录最小时间，不用每次都去MultiMap取第一个值
    private long minTime;

    private readonly Dictionary<long, Action<byte>> routerAckCallback = new();

    public void AddRouterAckCallback(long id, Action<byte> action)
    {
        this.routerAckCallback.Add(id, action);
    }

    public void RemoveRouterAckCallback(long id)
    {
        this.routerAckCallback.Remove(id);
    }

    public override IPEndPoint GetBindPoint()
    {
        return this.Transport.GetBindPoint();
    }

    public override void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        base.Dispose();

        foreach (var channelId in this.localConnectionChannels.Keys.ToArray())
        {
            this.Remove(channelId);
        }

        this.Transport.Dispose();
        this.Transport = null;
    }

    public override (uint, uint) GetChannelConnection(long channelId)
    {
        var channel = this.Get(channelId);
        if (channel == null)
        {
            throw new Exception($"GetChannelConn conn not found KChannel! {channelId}");
        }

        return (channel.LocalConn, channel.RemoteConn);
    }

    public override void ChangeAddress(long channelId, IPEndPoint newIPEndPoint)
    {
        var channel = this.Get(channelId);
        if (channel == null)
        {
            return;
        }

        channel.RemoteAddress = newIPEndPoint;
    }

    private void Recv()
    {
        if (this.Transport == null)
        {
            return;
        }

        while (this.Transport != null && this.Transport.Available() > 0)
        {
            var messageLength = this.Transport.Recv(this.cache, ref this.ipEndPoint);
            // 长度小于1，不是正常的消息
            if (messageLength < 1)
            {
                continue;
            }

            // accept
            var flag = this.cache[0];

            // conn从100开始，如果为1，2，3则是特殊包
            uint remoteConn = 0;
            uint localConn = 0;
            try
            {
                KcpChannel kChannel = null;
                switch (flag)
                {
                    case KcpProtocolType.RouterACK:
                    case KcpProtocolType.RouterReconnectACK:
                    {
                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);

                        var id = (long)(((ulong)localConn << 32) | remoteConn);
                        if (this.routerAckCallback.TryGetValue(id, out var action))
                        {
                            action.Invoke(flag);
                        }

                        break;
                    }
                    case KcpProtocolType.RouterReconnectSYN:
                    {
                        // 长度!=5，不是RouterReconnectSYN消息
                        if (messageLength != 9)
                        {
                            break;
                        }

                        string realAddress = null;
                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);

                        this.localConnectionChannels.TryGetValue(localConn, out kChannel);
                        if (kChannel == null)
                        {
                            Log.Warning("kchannel reconnect not found channel: {LocalConn} {RemoteConn} {RealAddress}", localConn, remoteConn, realAddress);
                            break;
                        }

                        // 这里必须校验localConn，客户端重连，localConn一定是一样的
                        if (localConn != kChannel.LocalConn)
                        {
                            Log.Warning("kchannel reconnect localconn error: {LocalConn} {RemoteConn} {RealAddress} {KChannelLocalConn}", localConn, remoteConn, realAddress, kChannel.LocalConn);
                            break;
                        }

                        if (remoteConn != kChannel.RemoteConn)
                        {
                            Log.Warning("kchannel reconnect remoteconn error: {LocalConn} {RemoteConn} {RealAddress} {KChannelRemoteConn}", localConn, remoteConn, realAddress, kChannel.RemoteConn);
                            break;
                        }

                        // 重连的时候router地址变化, 这个不能放到msg中，必须经过严格的验证才能切换
                        if (!this.ipEndPoint.Equals(kChannel.RemoteAddress))
                        {
                            kChannel.RemoteAddress = this.ipEndPoint.Clone();
                        }

                        try
                        {
                            var buffer = this.cache;
                            buffer.WriteTo(0, KcpProtocolType.RouterReconnectACK);
                            buffer.WriteTo(1, kChannel.LocalConn);
                            buffer.WriteTo(5, kChannel.RemoteConn);
                            this.Transport.Send(buffer, 0, 9, this.ipEndPoint, ChannelType.Accept);
                        }
                        catch (Exception exception)
                        {
                            Log.Error("{exception}", exception);
                            kChannel.OnError(StatusCodes.SocketCantSend);
                        }

                        break;
                    }
                    case KcpProtocolType.SYN: // accept
                    {
                        // 长度!=5，不是SYN消息
                        if (messageLength < 9)
                        {
                            break;
                        }

                        string realAddress = null;
                        if (messageLength > 9)
                        {
                            realAddress = this.cache.ToDefaultString(9, messageLength - 9);
                        }
                        else
                        {
                            realAddress = this.ipEndPoint.ToString();
                        }

                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);

                        this.waitAcceptChannels.TryGetValue(remoteConn, out kChannel);
                        if (kChannel == null)
                        {
                            while (true)
                            {
                                localConn = SharedRandom.NextUInt32();
                                if (!this.localConnectionChannels.ContainsKey(localConn))
                                {
                                    break;
                                }
                            }

                            kChannel = new KcpChannel(localConn, remoteConn, this.ipEndPoint.Clone(), this);
                            this.waitAcceptChannels.Add(kChannel.RemoteConn, kChannel); // 连接上了或者超时后会删除
                            this.localConnectionChannels.Add(kChannel.LocalConn, kChannel);

                            kChannel.RealAddress = realAddress;

                            var realEndPoint = NetworkAddressUtility.ToIPEndPoint(kChannel.RealAddress);
                            this.AcceptCallback(kChannel.Id, realEndPoint);
                        }

                        if (kChannel.RemoteConn != remoteConn)
                        {
                            break;
                        }

                        // 地址跟上次的不一致则跳过
                        if (kChannel.RealAddress != realAddress)
                        {
                            Log.Error($"kchannel syn address diff: {kChannel.Id} {kChannel.RealAddress} {realAddress}");
                            break;
                        }

                        try
                        {
                            var buffer = this.cache;
                            buffer.WriteTo(0, KcpProtocolType.ACK);
                            buffer.WriteTo(1, kChannel.LocalConn);
                            buffer.WriteTo(5, kChannel.RemoteConn);
                            Log.Information("kservice syn: {KChannelId} {RemoteConn} {LocalConn} {KChannelRemoteAddress}", kChannel.Id, remoteConn, localConn, kChannel.RemoteAddress);

                            this.Transport.Send(buffer, 0, 9, kChannel.RemoteAddress, ChannelType.Accept);
                        }
                        catch (Exception exception)
                        {
                            Log.Error("{exception}",exception);
                            kChannel.OnError(StatusCodes.SocketCantSend);
                        }

                        break;
                    }
                    case KcpProtocolType.ACK: // connect返回
                        // 长度!=9，不是connect消息
                        if (messageLength != 9)
                        {
                            break;
                        }

                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);
                        kChannel = this.Get(localConn);
                        if (kChannel != null)
                        {
                            Log.Information("kservice ack: {LocalConn} {RemoteConn}", localConn, remoteConn);
                            kChannel.RemoteConn = remoteConn;
                            kChannel.HandleConnnect();
                        }

                        break;
                    case KcpProtocolType.FIN: // 断开
                        // 长度!=13，不是DisConnect消息
                        if (messageLength != 13)
                        {
                            break;
                        }

                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);
                        var error = BitConverter.ToInt32(this.cache, 9);

                        // 处理chanel
                        kChannel = this.Get(localConn);
                        if (kChannel == null)
                        {
                            break;
                        }

                        // 校验remoteConn，防止第三方攻击
                        if (kChannel.RemoteConn != remoteConn)
                        {
                            break;
                        }

                        Log.Information("kservice recv fin: {LocalConn} {RemoteConn} {Error}", localConn, remoteConn, error);
                        kChannel.OnError(StatusCodes.PeerDisconnect);

                        break;
                    case KcpProtocolType.MSG: // 断开
                        // 长度<9，不是Msg消息
                        if (messageLength < 9)
                        {
                            break;
                        }

                        // 处理chanel
                        remoteConn = BitConverter.ToUInt32(this.cache, 1);
                        localConn = BitConverter.ToUInt32(this.cache, 5);

                        kChannel = this.Get(localConn);
                        if (kChannel == null)
                        {
                            // 通知对方断开
                            this.Disconnect(localConn, remoteConn, StatusCodes.KcpNotFoundChannel, this.ipEndPoint, 1);
                            break;
                        }

                        // 校验remoteConn，防止第三方攻击
                        if (kChannel.RemoteConn != remoteConn)
                        {
                            break;
                        }

                        // 对方发来msg，说明kchannel连接完成
                        if (!kChannel.IsConnected)
                        {
                            kChannel.IsConnected = true;
                            this.waitAcceptChannels.Remove(kChannel.RemoteConn);
                        }

                        kChannel.HandleRecv(this.cache, 5, messageLength - 5);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"kservice error: {flag} {remoteConn} {localConn}\n{e}");
            }
        }
    }

    public KcpChannel Get(long id)
    {
        this.localConnectionChannels.TryGetValue(id, out var channel);
        return channel;
    }

    public override void Create(long id, IPEndPoint ipEndPoint)
    {
        if (this.localConnectionChannels.TryGetValue(id, out var kChannel))
        {
            return;
        }

        try
        {
            // 低32bit是localConn
            var localConn = (uint)((ulong)id & uint.MaxValue);
            kChannel = new KcpChannel(localConn, ipEndPoint, this);
            this.localConnectionChannels.Add(kChannel.LocalConn, kChannel);
        }
        catch (Exception e)
        {
            Log.Error($"kservice get error: {id}\n{e}");
        }
    }

    public override void Remove(long id, int error = 0)
    {
        if (!this.localConnectionChannels.TryGetValue(id, out KcpChannel kChannel))
        {
            return;
        }

        kChannel.StatusCode = error;

        Log.Debug($"kservice remove channel: {id} {kChannel.LocalConn} {kChannel.RemoteConn} {error}");
        this.localConnectionChannels.Remove(kChannel.LocalConn);
        if (this.waitAcceptChannels.TryGetValue(kChannel.RemoteConn, out KcpChannel waitChannel))
        {
            if (waitChannel.LocalConn == kChannel.LocalConn)
            {
                this.waitAcceptChannels.Remove(kChannel.RemoteConn);
            }
        }

        kChannel.Dispose();
        this.Transport.OnError(id, error);
    }

    public void Disconnect(uint localConn, uint remoteConn, int error, EndPoint address, int times)
    {
        try
        {
            if (this.Transport == null)
            {
                return;
            }

            var buffer = this.cache;
            buffer.WriteTo(0, KcpProtocolType.FIN);
            buffer.WriteTo(1, localConn);
            buffer.WriteTo(5, remoteConn);
            buffer.WriteTo(9, (uint)error);
            for (var i = 0; i < times; ++i)
            {
                this.Transport.Send(buffer, 0, 13, address, ChannelType.Accept);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Disconnect error {localConn} {remoteConn} {error} {address} {e}");
        }

        Log.Information("channel send fin: {LocalConn} {RemoteConn} {EndPoint} {Error}", localConn, remoteConn, address, error);
    }

    public override void Send(long channelId, MemoryBuffer memoryBuffer)
    {
        var channel = this.Get(channelId);
        if (channel == null)
        {
            return;
        }

        channel.Send(memoryBuffer);
    }

    public override void Update()
    {
        var timeNow = this.TimeNow;

        this.TimerOut(timeNow);

        this.CheckWaitAcceptChannel(timeNow);

        this.Recv();

        this.UpdateChannel(timeNow);

        this.Transport.Update();
    }

    private void CheckWaitAcceptChannel(uint timeNow)
    {
        cacheIds.Clear();
        foreach (var kv in this.waitAcceptChannels)
        {
            var kChannel = kv.Value;
            if (kChannel.IsDisposed)
            {
                continue;
            }

            if (kChannel.IsConnected)
            {
                continue;
            }

            if (timeNow < kChannel.CreateTime + ConnectTimeout)
            {
                continue;
            }

            cacheIds.Add(kChannel.Id);
        }

        foreach (var id in this.cacheIds)
        {
            if (!this.waitAcceptChannels.TryGetValue(id, out var kChannel))
            {
                continue;
            }

            kChannel.OnError(StatusCodes.KcpAcceptTimeout);
        }
    }

    private void UpdateChannel(uint timeNow)
    {
        foreach (var id in this.updateIds)
        {
            var kChannel = this.Get(id);
            if (kChannel == null)
            {
                continue;
            }

            if (kChannel.Id == 0)
            {
                continue;
            }

            kChannel.Update(timeNow);
        }

        this.updateIds.Clear();
    }

    // 服务端需要看channel的update时间是否已到
    public void AddToUpdate(long time, long id)
    {
        if (time == 0)
        {
            this.updateIds.Add(id);
            return;
        }

        if (time < this.minTime)
        {
            this.minTime = time;
        }

        this.timeId.Add(time, id);
    }


    // 计算到期需要update的channel
    private void TimerOut(uint timeNow)
    {
        if (this.timeId.Count == 0)
        {
            return;
        }


        if (timeNow < this.minTime)
        {
            return;
        }

        this.timeOutTime.Clear();

        foreach (var kv in this.timeId)
        {
            var k = kv.Key;
            if (k > timeNow)
            {
                minTime = k;
                break;
            }

            this.timeOutTime.Add(k);
        }

        foreach (var k in this.timeOutTime)
        {
            foreach (var v in this.timeId[k])
            {
                this.updateIds.Add(v);
            }

            this.timeId.Remove(k);
        }
    }
}