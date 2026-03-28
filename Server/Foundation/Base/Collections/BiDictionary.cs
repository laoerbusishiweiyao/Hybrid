namespace Chaos;

public sealed class BiDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> keyValuePairs = new();
    private readonly Dictionary<TValue, TKey> valueKeyPairs = new();

    public BiDictionary()
    {
    }

    public BiDictionary(int capacity)
    {
        keyValuePairs = new Dictionary<TKey, TValue>(capacity);
        valueKeyPairs = new Dictionary<TValue, TKey>(capacity);
    }

    public void ForEach(Action<TKey, TValue> action)
    {
        if (action == null)
        {
            return;
        }

        var keys = keyValuePairs.Keys;
        foreach (var key in keys)
        {
            action(key, keyValuePairs[key]);
        }
    }

    public List<TKey> Keys => [..keyValuePairs.Keys];

    public List<TValue> Values => [..valueKeyPairs.Keys];

    public void Add(TKey key, TValue value)
    {
        if (key == null || value == null || keyValuePairs.ContainsKey(key) || valueKeyPairs.ContainsKey(value))
        {
            throw new Exception($"kv error or existed: {key} {value}");
        }

        keyValuePairs.Add(key, value);
        valueKeyPairs.Add(value, key);
    }

    public TValue GetValueOrDefault(TKey key)
    {
        return keyValuePairs.GetValueOrDefault(key);
    }

    public TKey GetKeyOrDefault(TValue value)
    {
        return valueKeyPairs.GetValueOrDefault(value);
    }

    public Dictionary<TKey, TValue> All => keyValuePairs;

    public void RemoveWithKey(TKey key)
    {
        if (key == null)
        {
            return;
        }

        if (!keyValuePairs.Remove(key, out var value))
        {
            return;
        }

        valueKeyPairs.Remove(value);
    }

    public void RemoveWithValue(TValue value)
    {
        if (value == null)
        {
            return;
        }

        if (!valueKeyPairs.TryGetValue(value, out var key))
        {
            return;
        }

        keyValuePairs.Remove(key);
        valueKeyPairs.Remove(value);
    }

    public void Clear()
    {
        keyValuePairs.Clear();
        valueKeyPairs.Clear();
    }

    public bool ContainsKey(TKey key)
    {
        return key != null && keyValuePairs.ContainsKey(key);
    }

    public bool ContainsValue(TValue value)
    {
        return value != null && valueKeyPairs.ContainsKey(value);
    }

    public bool Contains(TKey key, TValue value)
    {
        if (key == null || value == null)
        {
            return false;
        }

        return keyValuePairs.ContainsKey(key) && valueKeyPairs.ContainsKey(value);
    }
}