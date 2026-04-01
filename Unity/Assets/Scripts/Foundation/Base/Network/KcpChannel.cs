using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Chaos
{
    public sealed class KcpChannel : Channel
    {
        private const int MaxKcpMessageSize = 10000;

        private readonly KcpService service;

        private Kcp kcp { get; set; }

        private readonly Queue<MemoryBuffer> waitSendMessages = new();

        public readonly uint CreateTime;

        public uint LocalConn
        {
            get { return (uint)Id; }
            private set { Id = value; }
        }

        public uint RemoteConn { get; set; }

        private readonly byte[] sendCache = new byte[2 * 1024];

        public bool IsConnected { get; set; }

        public string RealAddress { get; set; }

        private MemoryBuffer readMemory;
        private int needReadSplitCount;

        private void InitKcp()
        {
            switch (service.ServiceType)
            {
                case ServiceType.Internal:
                    kcp.SetNoDelay(1, 1, 2, 1);
                    kcp.SetWindowSize(1024, 1024);
                    kcp.SetMtu(1400); // 默认1400
                    kcp.SetMinrto(30);
                    break;
                case ServiceType.External:
                    kcp.SetNoDelay(1, 1, 2, 1);
                    kcp.SetWindowSize(256, 256);
                    kcp.SetMtu(470);
                    kcp.SetMinrto(30);
                    break;
            }
        }

        // connect
        public KcpChannel(uint localConn, IPEndPoint remoteEndPoint, KcpService kService)
        {
            service = kService;
            LocalConn = localConn;
            ChannelType = ChannelType.Connect;

            Log.Information("channel create: {LocalConn} {RemoteEndPoint} {ChannelType}", LocalConn, remoteEndPoint, ChannelType);


            RemoteAddress = remoteEndPoint;
            CreateTime = kService.TimeNow;

            Connect(CreateTime);
        }

        // accept
        public KcpChannel(uint localConn, uint remoteConn, IPEndPoint remoteEndPoint, KcpService kService)
        {
            service = kService;
            ChannelType = ChannelType.Accept;

            Log.Information("channel create: {LocalConn} {RemoteConn} {RemoteEndPoint} {ChannelType}", localConn, remoteConn, remoteEndPoint, ChannelType);
            LocalConn = localConn;
            RemoteConn = remoteConn;
            RemoteAddress = remoteEndPoint;
            kcp = new Kcp(RemoteConn, Output);
            InitKcp();

            CreateTime = kService.TimeNow;
        }


        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            var localConn = LocalConn;
            var remoteConn = RemoteConn;
            Log.Information("channel dispose: {LocalConn} {RemoteConn} {Error}", localConn, remoteConn, StatusCode);

            var id = Id;
            Id = 0;
            service.Remove(id);

            try
            {
                if (StatusCode != StatusCodes.PeerDisconnect)
                {
                    service.Disconnect(localConn, remoteConn, StatusCode, RemoteAddress, 3);
                }
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }

            kcp = null;
        }

        public void HandleConnnect()
        {
            // 如果连接上了就不用处理了
            if (IsConnected)
            {
                return;
            }

            kcp = new Kcp(RemoteConn, Output);
            InitKcp();

            Log.Information($"channel connected: {LocalConn} {RemoteConn} {RemoteAddress} {waitSendMessages.Count}");
            IsConnected = true;
            while (true)
            {
                if (waitSendMessages.Count <= 0)
                {
                    break;
                }

                var buffer = waitSendMessages.Dequeue();
                Send(buffer);
            }
        }

        private long lastConnectTime = long.MaxValue;

        /// <summary>
        /// 发送请求连接消息
        /// </summary>
        private void Connect(uint timeNow)
        {
            try
            {
                if (IsConnected)
                {
                    return;
                }

                // 300毫秒后再次update发送connect请求
                if (timeNow < lastConnectTime + 300)
                {
                    service.AddToUpdate(300, Id);
                    return;
                }

                // 10秒连接超时
                if (timeNow > CreateTime + KcpService.ConnectTimeout)
                {
                    Log.Error($"kChannel connect timeout: {Id} {RemoteConn} {timeNow} {CreateTime} {ChannelType} {RemoteAddress}");
                    OnError(StatusCodes.KcpConnectTimeout);
                    return;
                }

                var buffer = sendCache;
                buffer.WriteTo(0, KcpProtocolType.SYN);
                buffer.WriteTo(1, LocalConn);
                buffer.WriteTo(5, RemoteConn);
                service.Transport.Send(buffer, 0, 9, RemoteAddress, ChannelType);
                // 这里很奇怪 调用socket.LocalEndPoint会动到this.RemoteAddressNonAlloc里面的temp，这里就不仔细研究了
                Log.Information($"kchannel connect {LocalConn} {RemoteConn} {RealAddress}");

                lastConnectTime = timeNow;

                service.AddToUpdate(300, Id);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
                OnError(StatusCodes.SocketCantSend);
            }
        }

        public void Update(uint timeNow)
        {
            if (IsDisposed)
            {
                return;
            }

            // 如果还没连接上，发送连接请求
            if (!IsConnected && ChannelType == ChannelType.Connect)
            {
                Connect(timeNow);
                return;
            }

            if (kcp == null)
            {
                return;
            }

            try
            {
                kcp.Update(timeNow);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
                OnError(StatusCodes.SocketError);
                return;
            }

            var nextUpdateTime = kcp.Check(timeNow);
            service.AddToUpdate(nextUpdateTime, Id);
        }

        public void HandleRecv(byte[] date, int offset, int length)
        {
            if (IsDisposed)
            {
                return;
            }

            kcp.Input(date.AsSpan(offset, length));
            service.AddToUpdate(0, Id);
            while (true)
            {
                if (IsDisposed)
                {
                    break;
                }

                var n = kcp.PeekSize();
                if (n < 0)
                {
                    break;
                }

                if (n == 0)
                {
                    OnError((int)SocketError.NetworkReset);
                    return;
                }

                if (needReadSplitCount > 0) // 说明消息分片了
                {
                    var buffer = readMemory.GetBuffer();
                    var count = kcp.Receive(buffer.AsSpan((int)(readMemory.Length - needReadSplitCount), n));
                    needReadSplitCount -= count;
                    if (n != count)
                    {
                        Log.Error("kchannel read error1: {LocalConn} {RemoteConn}", LocalConn, RemoteConn);
                        OnError(StatusCodes.KcpReadNotSame);
                        return;
                    }

                    if (needReadSplitCount < 0)
                    {
                        Log.Error("kchannel read error2: {LocalConn} {RemoteConn}", LocalConn, RemoteConn);
                        OnError(StatusCodes.KcpSplitError);
                        return;
                    }

                    // 没有读完
                    if (needReadSplitCount != 0)
                    {
                        continue;
                    }
                }
                else
                {
                    readMemory = service.Fetch(n);
                    readMemory.SetLength(n);
                    readMemory.Seek(0, SeekOrigin.Begin);

                    var buffer = readMemory.GetBuffer();

                    var count = kcp.Receive(buffer.AsSpan(0, n));
                    if (n != count)
                    {
                        break;
                    }

                    // 判断是不是分片
                    if (n == 8)
                    {
                        var headInt = BitConverter.ToInt32(readMemory.GetBuffer(), 0);
                        if (headInt == 0)
                        {
                            needReadSplitCount = BitConverter.ToInt32(readMemory.GetBuffer(), 4);
                            if (needReadSplitCount <= MaxKcpMessageSize)
                            {
                                Log.Error("kchannel read error3: {NeedReadSplitCount} {LocalConn} {RemoteConn}", needReadSplitCount, LocalConn, RemoteConn);
                                OnError(StatusCodes.KcpSplitCountError);
                                return;
                            }

                            readMemory.SetLength(needReadSplitCount);
                            readMemory.Seek(0, SeekOrigin.Begin);
                            continue;
                        }
                    }
                }

                var memoryBuffer = readMemory;
                readMemory = null;

                memoryBuffer.Seek(0, SeekOrigin.Begin);

                OnRead(memoryBuffer);
            }
        }

        private void Output(byte[] bytes, int count)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                // 没连接上 kcp不往外发消息, 其实本来没连接上不会调用update，这里只是做一层保护
                if (!IsConnected)
                {
                    return;
                }

                if (count == 0)
                {
                    Log.Error("output 0");
                    return;
                }

                bytes.WriteTo(0, KcpProtocolType.MSG);
                // 每个消息头部写下该channel的id;
                bytes.WriteTo(1, LocalConn);
                service.Transport.Send(bytes, 0, count + 5, RemoteAddress, ChannelType);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }

        private void KcpSend(MemoryBuffer memoryStream)
        {
            if (IsDisposed)
            {
                return;
            }

            var count = (int)(memoryStream.Length - memoryStream.Position);

            // 超出maxPacketSize需要分片

            if (count <= MaxKcpMessageSize)
            {
                kcp.Send(memoryStream.GetBuffer().AsSpan((int)memoryStream.Position, count));
            }
            else
            {
                // 先发分片信息
                sendCache.WriteTo(0, 0);
                sendCache.WriteTo(4, count);
                kcp.Send(sendCache.AsSpan(0, 8));

                // 分片发送
                var alreadySendCount = 0;
                while (alreadySendCount < count)
                {
                    var leftCount = count - alreadySendCount;

                    var sendCount = leftCount < MaxKcpMessageSize ? leftCount : MaxKcpMessageSize;

                    kcp.Send(memoryStream.GetBuffer().AsSpan((int)memoryStream.Position + alreadySendCount, sendCount));

                    alreadySendCount += sendCount;
                }
            }

            service.AddToUpdate(0, Id);
        }

        public void Send(MemoryBuffer memoryBuffer)
        {
            if (!IsConnected)
            {
                waitSendMessages.Enqueue(memoryBuffer);
                return;
            }

            if (kcp == null)
            {
                throw new Exception("kchannel connected but kcp is zero!");
            }

            // 检查等待发送的消息，如果超出最大等待大小，应该断开连接
            var n = (int)kcp.WaitSendCount;
            var maxWaitSize = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Internal:
                    maxWaitSize = Kcp.InnerMaxWaitSize;
                    break;
                case ServiceType.External:
                    maxWaitSize = Kcp.OuterMaxWaitSize;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (n > maxWaitSize)
            {
                Log.Error($"kcp wait snd too large: {n}: {LocalConn} {RemoteConn}");
                OnError(StatusCodes.KcpWaitSendSizeTooLarge);
                return;
            }

            KcpSend(memoryBuffer);
            service.Recycle(memoryBuffer);
        }

        private void OnRead(MemoryBuffer memoryStream)
        {
            try
            {
                service.ReadCallback(Id, memoryStream);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
                OnError(StatusCodes.PacketParserError);
            }
        }

        public void OnError(int error)
        {
            var channelId = Id;
            service.Remove(channelId, error);
            service.ErrorCallback(channelId, error);
        }
    }
}