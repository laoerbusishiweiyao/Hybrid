using System.Net;

namespace Chaos;

public enum ChannelType
{
    Connect,
    Accept,
}

public struct Packet
{
    public const int MinPacketSize = 2;
    public const int OpcodeLength = 2;
    public const int FiberInstanceIdIndex = 0;
    public const int FiberInstanceIdLength = 8;

    public ushort Opcode;
    public long ActorId;
    public MemoryStream MemoryStream;
}


public abstract class Channel : IDisposable
{
    public long Id;

    public ChannelType ChannelType { get; protected set; }

    public int StatusCode { get; set; }

    public IPEndPoint RemoteAddress { get; set; }

    public bool IsDisposed => this.Id == 0;

    public abstract void Dispose();
}