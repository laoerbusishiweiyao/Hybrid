using System.Diagnostics;

namespace Chaos;

internal sealed class ChildCollectionDebugTypeProxy(ChildCollection collection)
{
    private readonly ChildCollection collection = collection ?? throw new ArgumentNullException(nameof(collection));

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public Entity[] Entities
    {
        get
        {
            var entities = new Entity[collection.Count];
            collection.Values.CopyTo(entities, 0);
            return entities;
        }
    }
}

[DebuggerTypeProxy(typeof(ChildCollectionDebugTypeProxy))]
[DebuggerDisplay("Count = {Count}")]
public sealed class ChildCollection : SortedDictionary<long, Entity>, IPoolable
{
    public static ChildCollection Create(bool isFromPool = false)
    {
        return ObjectPool.Rent<ChildCollection>(isFromPool);
    }

    public bool IsFromPool { get; set; }

    public void Dispose()
    {
        if (!IsFromPool)
        {
            return;
        }

        IsFromPool = false;
        Clear();

        ObjectPool.Recycle(this);
    }
}