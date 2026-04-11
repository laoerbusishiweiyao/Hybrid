namespace Hybrid.Native.Windows;

public sealed class MessageAttribute(ushort opcode = 0) : Attribute
{
    public readonly ushort Opcode = opcode;
}