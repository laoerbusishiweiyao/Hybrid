using MongoDB.Bson;

namespace Chaos;

public abstract class Object
{
    public override string ToString()
    {
        return ((object)this).ToJson();
    }

    public string ToJson()
    {
        return MongoSerializer.ToJson(this);
    }

    public byte[] ToBson()
    {
        return MongoSerializer.Serialize(this);
    }
}