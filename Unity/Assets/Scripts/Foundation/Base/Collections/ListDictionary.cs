using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class ListDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public void Add(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            if (list == null)
            {
                list = new List<TValue>();
                base[key] = list;
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

        public new List<TValue> this[TKey key]
        {
            get
            {
                TryGetValue(key, out var list);
                return list;
            }
        }

        public TValue FirstOrDefault(TKey key)
        {
            TryGetValue(key, out var list);
            return list is { Count: > 0 } ? list[0] : default(TValue);
        }

        public bool Contains(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            return list != null && list.Contains(value);
        }
    }
}