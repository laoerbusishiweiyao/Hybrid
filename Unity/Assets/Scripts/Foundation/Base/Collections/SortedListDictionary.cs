using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class SortedListDictionary<TKey, TValue> : SortedDictionary<TKey, List<TValue>>
    {
        private readonly List<TValue> empty = new();
        private readonly int maxPoolCount;

        private readonly Queue<List<TValue>> poolQueue;

        public SortedListDictionary(int maxPoolCount)
        {
            this.maxPoolCount = maxPoolCount;
            poolQueue = new Queue<List<TValue>>(maxPoolCount);
        }

        private List<TValue> RentList()
        {
            return poolQueue.Count > 0 ? poolQueue.Dequeue() : new List<TValue>(10);
        }

        private void Recycle(List<TValue> list)
        {
            if (list == null)
            {
                return;
            }

            if (poolQueue.Count == maxPoolCount)
            {
                return;
            }

            list.Clear();
            poolQueue.Enqueue(list);
        }

        public void Add(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            if (list == null)
            {
                list = RentList();
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

        public new bool Remove(TKey key)
        {
            TryGetValue(key, out var list);
            if (list == null)
            {
                return false;
            }

            Recycle(list);
            return base.Remove(key);
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
                return list ?? empty;
            }
        }

        public TValue FirstOrDefault(TKey key)
        {
            TryGetValue(key, out var list);
            return list is { Count: > 0 } ? list[0] : default;
        }

        public bool Contains(TKey key, TValue value)
        {
            TryGetValue(key, out var list);
            return list != null && list.Contains(value);
        }
    }
}