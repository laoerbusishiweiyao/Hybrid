namespace Chaos
{
    public sealed record WebMessageInfo
    {
        public ushort Opcode;
        public object Payload;
    }
}