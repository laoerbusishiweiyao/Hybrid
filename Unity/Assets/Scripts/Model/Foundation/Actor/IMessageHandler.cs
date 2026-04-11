using System;

namespace Chaos
{
    public interface IMessageHandler
    {
        ThreadTask Handle(Entity entity, int fromFiber, MessageObject messageObject);
        Type RequestType { get; }
        Type ResponseType { get; }
    }
}