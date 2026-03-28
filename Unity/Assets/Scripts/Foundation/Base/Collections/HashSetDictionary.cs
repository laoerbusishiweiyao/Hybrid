using System.Collections.Generic;
using System.Linq;

namespace Chaos
{
    public sealed class HashSetDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        public new HashSet<TValue> this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var set))
                {
                    set = new HashSet<TValue>();
                }

                return set;
            }
        }

        public Dictionary<TKey, HashSet<TValue>> GetDictionary()
        {
            return this;
        }

        public void Add(TKey key, TValue value)
        {
            TryGetValue(key, out var set);
            if (set == null)
            {
                set = new HashSet<TValue>();
                base[key] = set;
            }

            set.Add(value);
        }

        public bool Remove(TKey key, TValue value)
        {
            TryGetValue(key, out var set);
            if (set == null)
            {
                return false;
            }

            if (!set.Remove(value))
            {
                return false;
            }

            if (set.Count == 0)
            {
                Remove(key);
            }

            return true;
        }

        public bool Contains(TKey key, TValue value)
        {
            TryGetValue(key, out var set);
            return set != null && set.Contains(value);
        }

        public new int Count => this.Sum(pair => pair.Value.Count);
    }
}