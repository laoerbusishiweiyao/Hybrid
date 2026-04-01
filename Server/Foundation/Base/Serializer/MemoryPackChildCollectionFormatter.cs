using MemoryPack.Internal;
using MemoryPack;

namespace Chaos;

[Preserve]
public sealed class MemoryPackChildCollectionFormatter : MemoryPackFormatter<ChildCollection>
{
    [Preserve]
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ChildCollection value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        // writer.WriteCollectionHeader(value.Count);
        var formatter = writer.GetFormatter<Entity>();
        ref byte spanReference = ref writer.GetSpanReference(4);
        writer.Advance(4);
        int count = 0;
        foreach (var kv in value)
        {
            Entity entity = kv.Value;
            if (entity.SerializeWithParent)
            {
                ++count;
                formatter.Serialize(ref writer, ref entity!);
            }
        }

        SafeUnsafe.WriteUnaligned(ref spanReference, count);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, scoped ref ChildCollection value)
    {
        if (!reader.TryReadCollectionHeader(out int length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = ChildCollection.Create(true);
        }
        else
        {
            value.Clear();
        }

        var formatter = reader.GetFormatter<Entity>();
        for (int i = 0; i < length; i++)
        {
            Entity entity = null!;
            formatter.Deserialize(ref reader, ref entity!);
            entity.SerializeWithParent = true;
            value.Add(entity.Id, entity);
        }
    }
}