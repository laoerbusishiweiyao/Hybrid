using System.Net;

namespace Chaos;

public interface IKcpTransport : IDisposable
{
    void Send(byte[] bytes, int index, int length, EndPoint point, ChannelType channelType);
    int Recv(byte[] buffer, ref EndPoint point);
    IPEndPoint GetBindPoint();
    int Available();
    void Update();
    void OnError(long id, int code);
}