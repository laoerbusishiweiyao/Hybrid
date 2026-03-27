using System;

namespace Chaos
{
    public interface IWebMessageHandler
    {
        Type MessageType { get; }
        Type ResponseType { get; }
        void Handle(object message);
    }
}