using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class TypeSystemGroup
    {
        public sealed class TypeSystemCollection
        {
            public readonly ListDictionary<Type, SystemObject> SystemHandlers = new();
            public readonly List<Type> ClassEventTypes = new();
        }

        private readonly Dictionary<Type, TypeSystemCollection> collections = new();

        public TypeSystemCollection FindOrCreate(Type type)
        {
            collections.TryGetValue(type, out var collection);
            if (collection != null)
            {
                return collection;
            }

            collection = new TypeSystemCollection();
            collections.Add(type, collection);
            return collection;
        }

        public TypeSystemCollection Find(Type type)
        {
            collections.TryGetValue(type, out var systems);
            return systems;
        }

        public List<SystemObject> Find(Type type, Type systemType)
        {
            return !collections.TryGetValue(type, out var collection) ? null : collection.SystemHandlers.GetValueOrDefault(systemType);
        }
    }
}