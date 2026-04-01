using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Chaos
{
    public sealed class BsonComponentCollectionSerializer : IBsonSerializer
    {
        public BsonComponentCollectionSerializer(Type valueType)
        {
            ValueType = valueType;
        }

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var collection = ComponentCollection.Create(true);
            var serializer = BsonSerializer.LookupSerializer<Entity>();
            var reader = context.Reader;
            reader.ReadStartArray();
            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var entity = serializer.Deserialize(context);
                entity.SerializeWithParent = true;
                collection.Add(entity.GetLongHashCode(), entity);
            }

            reader.ReadEndArray();

            return collection;
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            using var writer = context.Writer;
            writer.WriteStartArray();
            var collection = (ComponentCollection)value;

            var serializer = BsonSerializer.LookupSerializer<Entity>();
            foreach (var (_, entity) in collection)
            {
                if (entity.SerializeWithParent)
                {
                    serializer.Serialize(context, entity);
                }
            }

            writer.WriteEndArray();
        }

        public Type ValueType { get; }
    }
}