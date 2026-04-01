using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Chaos;

public static class MongoRegister
{
    public static void RegisterStruct<T>() where T : struct
    {
        BsonSerializer.RegisterSerializer(typeof(T), new BsonStructSerializer<T>());
    }

    public static void Initialize()
    {
        // 清理老的数据
        var createSerializerRegistry = typeof(BsonSerializer).GetMethod("CreateSerializerRegistry", BindingFlags.Static | BindingFlags.NonPublic);
        createSerializerRegistry.Invoke(null, Array.Empty<object>());
        var registerIdGenerators = typeof(BsonSerializer).GetMethod("RegisterIdGenerators", BindingFlags.Static | BindingFlags.NonPublic);
        registerIdGenerators.Invoke(null, Array.Empty<object>());

        ObjectSerializer objectSerializer = new(_ => true);
        BsonSerializer.RegisterSerializer(objectSerializer);

        BsonSerializer.RegisterSerializer(typeof(ComponentCollection), new BsonComponentCollectionSerializer(typeof(ComponentCollection)));
        BsonSerializer.RegisterSerializer(typeof(ChildCollection), new BsonChildCollectionSerializer(typeof(ChildCollection)));

        // 自动注册IgnoreExtraElements
        ConventionPack conventionPack = new() { new IgnoreExtraElementsConvention(true) };

        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        foreach (var type in CodeTypeRegistry.Default.GetTypes().Values)
        {
            RegisterClass(type);
        }
    }

    private static void RegisterClass(Type type)
    {
        if (!type.IsSubclassOf(typeof(Object)))
        {
            return;
        }

        if (type.IsGenericType)
        {
            return;
        }

        if (BsonClassMap.IsClassMapRegistered(type))
        {
            return;
        }

        BsonClassMap cm = new(type);
        cm.AutoMap();
        cm.SetDiscriminator(type.FullName);
        BsonClassMap.RegisterClassMap(cm);
    }
}