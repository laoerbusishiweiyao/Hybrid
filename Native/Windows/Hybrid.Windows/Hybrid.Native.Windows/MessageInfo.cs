namespace Hybrid.Native.Windows;

public sealed record MessageInfo
{
    public required ushort Opcode;
    public required object Payload;
}