using System;

namespace Chaos
{
    public interface ISessionMessageHandler
    {
        void Handle(Session session, object message);
        
        Type MessageType { get; }
        Type ResponseType { get; }
    }
}