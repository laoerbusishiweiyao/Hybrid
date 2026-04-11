namespace Chaos
{
    [InvokeHandler(MailboxType.UnorderedMessage)]
    public sealed class MailBoxType_UnOrderedMessageHandler : InvokeHandler<MailboxInvokeEventArgs>
    {
        public override void Handle(MailboxInvokeEventArgs eventArgs)
        {
            MailboxComponent mailboxComponent = eventArgs.MailboxComponent;

            var messageObject = eventArgs.MessageObject;

            MessageDispatcher.Default.Handle(mailboxComponent.Parent, eventArgs.FromFiber, messageObject);
        }
    }
}