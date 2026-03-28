namespace Chaos;

public sealed class SortedHashSetDictionary<TKey, TValue>(int maxPoolCount = 0) : SortedDictionary<TKey, HashSet<TValue>>
{
    private readonly HashSet<TValue> empty = [];

    public void Add(TKey key, TValue value)
    {
        TryGetValue(key, out var list);
        if (list == null)
        {
            list = [];
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
        return list == null ? [] : list.ToArray();
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