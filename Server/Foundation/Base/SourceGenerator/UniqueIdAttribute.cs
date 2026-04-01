namespace Chaos;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class UniqueIdAttribute(int min = int.MinValue, int max = int.MaxValue) : Attribute
{
    public readonly int Min = min;
    public readonly int Max = max;
}