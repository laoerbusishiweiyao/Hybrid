using System.Collections.Generic;

namespace Chaos
{
    [ComponentOf(typeof(Scene))]
    public sealed class ProcessInnerSender : Entity, IAwake, IDestroy, IUpdate
    {
        public const long Timeout = 40 * 1000;

        public int RequestId;

        public readonly Dictionary<int, ProcessInnerMessageSenderInfo> RequestCallback = new();

        public readonly List<MessageInfo> MessageInfos = new();
    }
}