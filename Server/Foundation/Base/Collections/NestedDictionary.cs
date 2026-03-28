namespace Chaos;

public sealed class NestedDictionary<TKey, TSubKey, TValue> : Dictionary<TKey, Dictionary<TSubKey, TValue>>
{
    public bool TryGetDictionary(TKey key, out Dictionary<TSubKey, TValue> dictionary)
    {
        return TryGetValue(key, out dictionary);
    }

    public bool TryGetValue(TKey key, TSubKey subKey, out TValue value)
    {
        value = default;
        return TryGetValue(key, out var dictionary) && dictionary.TryGetValue(subKey, out value);
    }

    public void Add(TKey key, TSubKey subKey, TValue value)
    {
        TryGetValue(key, out var dictionary);
        if (dictionary == null)
        {
            dictionary = new Dictionary<TSubKey, TValue>();
            this[key] = dictionary;
        }

        dictionary.Add(subKey, value);
    }

    public bool Remove(TKey key, TSubKey subKey)
    {
        TryGetValue(key, out var dictionary);
        if (dictionary == null || !dictionary.Remove(subKey))
        {
            return false;
        }

        if (dictionary.Count == 0)
        {
            Remove(key);
        }

        return true;
    }

    public bool Contains(TKey key, TSubKey subKey)
    {
        TryGetValue(key, out var dictionary);
        return dictionary != null && dictionary.ContainsKey(subKey);
    }

    public bool Contains(TKey key, TSubKey subKey, TValue value)
    {
        TryGetValue(key, out var dictionary);
        if (dictionary == null)
        {
            return false;
        }

        return dictionary.ContainsKey(subKey) && dictionary.ContainsValue(value);
    }
}