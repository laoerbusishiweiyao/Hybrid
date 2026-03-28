using MongoDB.Bson.Serialization.Attributes;

namespace Chaos
{
    public abstract class MessageObject : ProtoObject, IMessage, IPoolable
    {
        public virtual void Dispose()
        {
        }

        [BsonIgnore] public bool IsFromPool { get; set; }
    }
}