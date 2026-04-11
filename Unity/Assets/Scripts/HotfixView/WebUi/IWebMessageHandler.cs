using System;

namespace Chaos
{
    public interface IWebMessageHandler
    {
        void Handle(WebUiComponent webUiComponent, MessageObject messageObject);
        Type RequestType { get; }
        Type ResponseType { get; }
    }
}