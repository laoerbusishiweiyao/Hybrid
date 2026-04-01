using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Chaos;

public sealed class UdpTransport : IKcpTransport
{
    private readonly Socket socket;

    public UdpTransport(AddressFamily addressFamily)
    {
        socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
        NetworkAddressUtility.SetSioUdpConnReset(socket);
    }

    public UdpTransport(IPEndPoint ipEndPoint)
    {
        socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            socket.SendBufferSize = Kcp.OneM * 64;
            socket.ReceiveBufferSize = Kcp.OneM * 64;
        }

        try
        {
            socket.Bind(ipEndPoint);
        }
        catch (Exception e)
        {
            throw new Exception($"bind error: {ipEndPoint}", e);
        }

        NetworkAddressUtility.SetSioUdpConnReset(socket);
    }

    public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
    {
        socket.SendTo(bytes, index, length, SocketFlags.None, endPoint);
    }

    public int Recv(byte[] buffer, ref EndPoint endPoint)
    {
        return socket.ReceiveFrom(buffer, ref endPoint);
    }

    public IPEndPoint GetBindPoint()
    {
        return socket.LocalEndPoint as IPEndPoint;
    }

    public int Available()
    {
        return socket.Available;
    }

    public void Update()
    {
    }

    public void OnError(long id, int error)
    {
    }

    public void Dispose()
    {
        socket?.Dispose();
    }
}