using System.ComponentModel;

namespace Chaos;

public abstract class ProtoObject : Object, ISupportInitialize
{
    public object Clone()
    {
        var buffer = MongoSerializer.Serialize(this);
        return MongoSerializer.Deserialize(GetType(), buffer, 0, buffer.Length);
    }

    public virtual void BeginInit()
    {
    }

    public virtual void EndInit()
    {
    }
}