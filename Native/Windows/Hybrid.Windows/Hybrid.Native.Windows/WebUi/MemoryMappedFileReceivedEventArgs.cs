namespace Hybrid.Native.Windows;

public sealed class MemoryMappedFileReceivedEventArgs(ushort opcode, string payload) : EventArgs
{
    public readonly ushort Opcode = opcode;
    public readonly string Payload = payload;
}