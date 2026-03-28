using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Chaos;

public static class MongoSerializer
{
    private static readonly JsonWriterSettings defaultJsonWriteSettings = new()
    {
        OutputMode = JsonOutputMode.RelaxedExtendedJson
    };

    public static readonly JsonWriterSettings ConfigJsonWriterSettings = new()
    {
        Indent = true,
        IndentChars = "\t",
        NewLineChars = "\n",
        OutputMode = JsonOutputMode.Shell,
    };

    public static string ToJson(object instance)
    {
        try
        {
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return instance.ToJson(defaultJsonWriteSettings);
        }
        catch (Exception exception)
        {
            throw new Exception($"to json error {instance.GetType().FullName}\n{exception}");
        }
    }

    public static string ToJson(object instance, JsonWriterSettings settings)
    {
        try
        {
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return instance.ToJson(settings);
        }
        catch (Exception exception)
        {
            throw new Exception($"to json error {instance.GetType().FullName}\n{exception}");
        }
    }

    public static T FromJson<T>(string content)
    {
        try
        {
            return BsonSerializer.Deserialize<T>(content);
        }
        catch (Exception exception)
        {
            throw new Exception($"from json error: {content}\n{exception}");
        }
    }

    public static object FromJson(Type type, string content)
    {
        try
        {
            return BsonSerializer.Deserialize(content, type);
        }
        catch (Exception e)
        {
            throw new Exception($"from json error: {content}\n{e}");
        }
    }

    public static byte[] Serialize(object instance)
    {
        try
        {
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            return instance.ToBson();
        }
        catch (Exception exception)
        {
            throw new Exception($"Serialize error {instance.GetType().FullName}\n{exception}");
        }
    }

    public static void Serialize(object instance, MemoryStream stream)
    {
        try
        {
            if (instance is ISupportInitialize supportInitialize)
            {
                supportInitialize.BeginInit();
            }

            using BsonBinaryWriter writer = new(stream, BsonBinaryWriterSettings.Defaults);
            var context = BsonSerializationContext.CreateRoot(writer);
            BsonSerializationArgs args = default;
            args.NominalType = typeof(object);
            var serializer = BsonSerializer.LookupSerializer(args.NominalType);
            serializer.Serialize(context, args, instance);
        }
        catch (Exception exception)
        {
            throw new Exception($"Serialize error {instance.GetType().FullName}\n{exception}");
        }
    }

    public static object Deserialize(Type type, byte[] buffer)
    {
        try
        {
            return BsonSerializer.Deserialize(buffer, type);
        }
        catch (Exception exception)
        {
            throw new Exception($"from bson error: {type.FullName} {buffer.Length}", exception);
        }
    }

    public static object Deserialize(Type type, byte[] buffer, int index, int count)
    {
        try
        {
            using MemoryStream stream = new(buffer, index, count);
            return BsonSerializer.Deserialize(stream, type);
        }
        catch (Exception exception)
        {
            throw new Exception($"from bson error: {type.FullName} {buffer.Length} {index} {count}", exception);
        }
    }

    public static object Deserialize(Type type, Stream stream)
    {
        try
        {
            return BsonSerializer.Deserialize(stream, type);
        }
        catch (Exception exception)
        {
            throw new Exception($"from bson error: {type.FullName} {stream.Position} {stream.Length}", exception);
        }
    }

    public static T Deserialize<T>(byte[] buffer)
    {
        try
        {
            using MemoryStream memoryStream = new(buffer);
            return BsonSerializer.Deserialize<T>(memoryStream);
        }
        catch (Exception exception)
        {
            throw new Exception($"from bson error: {typeof(T).FullName} {buffer.Length}", exception);
        }
    }

    public static T Deserialize<T>(byte[] buffer, int index, int count)
    {
        return (T)Deserialize(typeof(T), buffer, index, count);
    }

    public static T Clone<T>(T instance)
    {
        return Deserialize<T>(Serialize(instance!));
    }
}