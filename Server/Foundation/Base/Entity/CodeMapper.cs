using System.Reflection;

namespace Chaos;

public abstract class CodeMapper<T> : Singleton<T> where T : CodeMapper<T>
{
    protected readonly BiDictionary<int, string> valueStringPairs = new();

    protected void Initialize(Type type)
    {
        var infos = type.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var info in infos)
        {
            if (info.FieldType != typeof(int))
            {
                continue;
            }

            valueStringPairs.Add((int)info.GetValue(null)!, info.Name);
        }
    }

    public string GetStringByValue(int value)
    {
        return valueStringPairs.GetValueOrDefault(value);
    }

    public int GetValueByName(string name)
    {
        return valueStringPairs.GetKeyOrDefault(name);
    }

    public BiDictionary<int, string> All => valueStringPairs;
}