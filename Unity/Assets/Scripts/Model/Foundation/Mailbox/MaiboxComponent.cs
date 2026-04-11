namespace Chaos
{
    [EntitySystemOf(typeof(MailboxComponent))]
    public static partial class MailboxComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MailboxComponent self, int mailBoxType)
        {
            var fiber = self.Fiber();
            self.MailboxType = mailBoxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            fiber.Mailboxes.Add(self);
        }

        [EntitySystem]
        private static void Destroy(this MailboxComponent self)
        {
            self.Fiber().Mailboxes.Remove(self.ParentInstanceId);
        }

        public static void Add(this MailboxComponent self, int fromFiber, MessageObject messageObject)
        {
            EventSystem.Default.Invoke(self.MailboxType, new MailboxInvokeEventArgs(fromFiber, messageObject, self));
        }
    }

    public readonly struct MailboxInvokeEventArgs
    {
        public readonly int FromFiber;
        public readonly MessageObject MessageObject;
        public readonly EntityReference<MailboxComponent> MailboxComponent;

        public MailboxInvokeEventArgs(int fromFiber, MessageObject messageObject, EntityReference<MailboxComponent> mailboxComponent)
        {
            FromFiber = fromFiber;
            MessageObject = messageObject;
            MailboxComponent = mailboxComponent;
        }
    }

    [ComponentOf]
    public sealed class MailboxComponent : Entity, IAwake<int>, IDestroy
    {
        public long ParentInstanceId { get; set; }
        public int MailboxType { get; set; }
    }
}