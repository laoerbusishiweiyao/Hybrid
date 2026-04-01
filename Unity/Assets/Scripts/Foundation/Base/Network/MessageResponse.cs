using MemoryPack;

namespace Chaos
{
    [Message(ushort.MaxValue)]
    [MemoryPackable]
    public sealed partial class MessageResponse : MessageObject, IResponse
    {
        [MemoryPackOrder(1)]
        public int RequestId { get; set; }

        [MemoryPackOrder(2)]
        public int StatusCode { get; set; }

        [MemoryPackOrder(3)]
        public string Message { get; set; }
    }
}