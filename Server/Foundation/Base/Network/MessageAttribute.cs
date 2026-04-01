namespace Chaos;

public sealed class MessageAttribute : BaseAttribute
{
    public readonly ushort Opcode;

    public MessageAttribute(ushort opcode = 0)
    {
        Opcode = opcode;
    }
}