namespace Chaos
{
    public readonly struct WebUiInitializeFinishEventArgs
    {
    }

    public readonly struct WebUiMessageSentEventArgs
    {
        public readonly IMessage Message;

        public WebUiMessageSentEventArgs(IMessage message)
        {
            Message = message;
        }
    }
}