using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chaos
{
    internal sealed class ComponentCollectionDebugTypeProxy
    {
        private readonly ComponentCollection collection;

        public ComponentCollectionDebugTypeProxy(ComponentCollection collection)
        {
            this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Entity[] Components
        {
            get
            {
                var components = new Entity[collection.Count];
                collection.Values.CopyTo(components, 0);
                return components;
            }
        }
    }

    [DebuggerTypeProxy(typeof(ComponentCollectionDebugTypeProxy))]
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ComponentCollection : SortedDictionary<long, Entity>, IPoolable
    {
        public static ComponentCollection Create(bool isFromPool = false)
        {
            return ObjectPool.Rent<ComponentCollection>(isFromPool);
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
}