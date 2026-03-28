using System;
using System.Collections.Generic;
using System.Linq;

namespace Chaos
{
    public sealed class SortedHashSetDictionary<TKey, TValue> : SortedDictionary<TKey, HashSet<TValue>>
    {
        private readonly HashSet<TValue> empty = new();

        public void Add(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            if (list == null)
            {
                list = new HashSet<TValue>();
                Add(key, list);
            }

            list.Add(value);
        }

        public bool Remove(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            if (list == null)
            {
                return false;
            }

            if (!list.Remove(value))
            {
                return false;
            }

            if (list.Count == 0)
            {
                Remove(key);
            }

            return true;
        }

        public TValue[] GetAll(TKey key)
        {
            TryGetValue(key, out var list);
            return list == null ? Array.Empty<TValue>() : list.ToArray();
        }

        public new HashSet<TValue> this[TKey key]
        {
            get
            {
                TryGetValue(key, out var list);
                return list ?? empty;
            }
        }

        public TValue FirstOrDefault(TKey key)
        {
            TryGetValue(key, out var list);
            return list is { Count: > 0 } ? list.FirstOrDefault() : default;
        }

        public bool Contains(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            return list != null && list.Contains(value);
        }
    }
}