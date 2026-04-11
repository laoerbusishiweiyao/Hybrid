namespace Chaos
{
    public sealed record WebMessageInfo
    {
        public readonly ushort Opcode;
        public readonly object Payload;

        public WebMessageInfo(ushort opcode, object payload)
        {
            Opcode = opcode;
            Payload = payload;
        }
    }
}