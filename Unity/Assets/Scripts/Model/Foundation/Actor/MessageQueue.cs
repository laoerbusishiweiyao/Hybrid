using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Chaos
{
    public readonly struct MessageInfo
    {
        public readonly FiberInstanceId FiberInstanceId;
        public readonly MessageObject MessageObject;

        public MessageInfo(FiberInstanceId fiberInstanceId, MessageObject messageObject)
        {
            FiberInstanceId = fiberInstanceId;
            MessageObject = messageObject;
        }
    }

    public sealed class MessageQueue : Singleton<MessageQueue>, ISingletonAwake
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<MessageInfo>> messages = new();

        public void Awake()
        {
        }

        public bool Send(Fiber fiber, FiberInstanceId fiberInstanceId, MessageObject messageObject)
        {
            NetworkLogger.Default.Send(fiber, messageObject);
            if (!messages.TryGetValue(fiberInstanceId.Fiber, out var queue))
            {
                return false;
            }

            queue.Enqueue(new MessageInfo(new FiberInstanceId(fiber.Id, fiberInstanceId.InstanceId), messageObject));
            return true;
        }

        public void Fetch(Fiber fiber, int count, List<MessageInfo> list)
        {
            if (!messages.TryGetValue(fiber.Id, out var queue))
            {
                return;
            }

            for (var i = 0; i < count; ++i)
            {
                if (!queue.TryDequeue(out var message))
                {
                    break;
                }

                NetworkLogger.Default.Recv(fiber, message.MessageObject);
                list.Add(message);
            }
        }

        public void AddQueue(int fiberId)
        {
            var queue = new ConcurrentQueue<MessageInfo>();
            messages[fiberId] = queue;
        }

        public void RemoveQueue(int fiberId)
        {
            messages.TryRemove(fiberId, out _);
        }
    }
}