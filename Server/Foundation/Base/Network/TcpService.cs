using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Chaos;

public enum TcpOperation
{
    StartSend,
    StartRecv,
    Connect,
}

public struct TcpEventArgs
{
    public TcpOperation Operation;
    public long ChannelId;
    public SocketAsyncEventArgs SocketAsyncEventArgs;
}

public sealed class TcpService : Service
{
    private readonly Dictionary<long, TcpChannel> idChannels = new();

    private readonly SocketAsyncEventArgs innArgs = new();

    private Socket acceptor;

    public ConcurrentQueue<TcpEventArgs> Queue = new();

    private long idGenerator = 1;

    public long NewId() => idGenerator++;

    public TcpService(AddressFamily addressFamily, ServiceType serviceType)
    {
        ServiceType = serviceType;
    }

    public TcpService(IPEndPoint ipEndPoint, ServiceType serviceType)
    {
        ServiceType = serviceType;
        acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        // 容易出问题，先注释掉，按需开启
        //this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        innArgs.Completed += OnComplete;
        try
        {
            acceptor.Bind(ipEndPoint);
        }
        catch (Exception e)
        {
            throw new Exception($"bind error: {ipEndPoint}", e);
        }

        acceptor.Listen(1000);

        AcceptAsync();
    }

    public override IPEndPoint GetBindPoint()
    {
        return acceptor.LocalEndPoint as IPEndPoint;
    }

    private void OnComplete(object sender, SocketAsyncEventArgs e)
    {
        switch (e.LastOperation)
        {
            case SocketAsyncOperation.Accept:
                Queue.Enqueue(new TcpEventArgs() { SocketAsyncEventArgs = e });
                break;
            default:
                throw new Exception($"socket error: {e.LastOperation}");
        }
    }

    private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
    {
        if (acceptor == null)
        {
            return;
        }

        if (socketError != SocketError.Success)
        {
            Log.Error($"accept error {socketError}");
            AcceptAsync();
            return;
        }

        try
        {
            var channelId = NewId();
            TcpChannel channel = new(channelId, acceptSocket, this);
            idChannels.Add(channel.Id, channel);

            AcceptCallback(channelId, channel.RemoteAddress);
        }
        catch (Exception exception)
        {
            Log.Error("{e}", exception);
        }

        // 开始新的accept
        AcceptAsync();
    }

    private void AcceptAsync()
    {
        innArgs.AcceptSocket = null;
        if (acceptor.AcceptAsync(innArgs))
        {
            return;
        }

        OnAcceptComplete(innArgs.SocketError, innArgs.AcceptSocket);
    }

    public override void Create(long id, IPEndPoint ipEndPoint)
    {
        if (idChannels.TryGetValue(id, out var _))
        {
            return;
        }

        TcpChannel channel = new(id, ipEndPoint, this);
        idChannels.Add(channel.Id, channel);
    }

    public TcpChannel Get(long id)
    {
        idChannels.TryGetValue(id, out var channel);
        return channel;
    }

    public override void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        base.Dispose();

        acceptor?.Close();
        acceptor = null;
        innArgs.Dispose();

        foreach (var id in idChannels.Keys.ToArray())
        {
            var channel = idChannels[id];
            channel.Dispose();
        }
    }

    public override void Remove(long id, int error = 0)
    {
        if (idChannels.TryGetValue(id, out var channel))
        {
            channel.StatusCode = error;
            channel.Dispose();
        }

        idChannels.Remove(id);
    }

    public override void Send(long channelId, MemoryBuffer memoryBuffer)
    {
        try
        {
            var aChannel = Get(channelId);
            if (aChannel == null)
            {
                ErrorCallback(channelId, StatusCodes.SendMessageNotFoundTChannel);
                return;
            }

            aChannel.Send(memoryBuffer);
        }
        catch (Exception exception)
        {
            Log.Error("{exception}", exception);
        }
    }

    public override void Update()
    {
        while (true)
        {
            if (!Queue.TryDequeue(out var result))
            {
                break;
            }

            var e = result.SocketAsyncEventArgs;
            if (e == null)
            {
                switch (result.Operation)
                {
                    case TcpOperation.StartSend:
                    {
                        var channel = Get(result.ChannelId);
                        channel?.StartSend();
                        break;
                    }
                    case TcpOperation.StartRecv:
                    {
                        var channel = Get(result.ChannelId);
                        channel?.StartRecv();
                        break;
                    }
                    case TcpOperation.Connect:
                    {
                        var channel = Get(result.ChannelId);
                        channel?.ConnectAsync();
                        break;
                    }
                }

                continue;
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                {
                    var socketError = e.SocketError;
                    var acceptSocket = e.AcceptSocket;
                    OnAcceptComplete(socketError, acceptSocket);
                    break;
                }
                case SocketAsyncOperation.Connect:
                {
                    var TcpChannel = Get(result.ChannelId);
                    if (TcpChannel != null)
                    {
                        TcpChannel.OnConnectComplete(e);
                    }

                    break;
                }
                case SocketAsyncOperation.Disconnect:
                {
                    var TcpChannel = Get(result.ChannelId);
                    if (TcpChannel != null)
                    {
                        TcpChannel.OnDisconnectComplete(e);
                    }

                    break;
                }
                case SocketAsyncOperation.Receive:
                {
                    var TcpChannel = Get(result.ChannelId);
                    if (TcpChannel != null)
                    {
                        TcpChannel.OnRecvComplete(e);
                    }

                    break;
                }
                case SocketAsyncOperation.Send:
                {
                    var TcpChannel = Get(result.ChannelId);
                    if (TcpChannel != null)
                    {
                        TcpChannel.OnSendComplete(e);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"{e.LastOperation}");
            }
        }
    }
}