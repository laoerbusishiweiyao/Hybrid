namespace Chaos;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EntitySystemOfAttribute(Type type, bool ignoreAwake = false) : BaseAttribute
{
    public readonly Type Type = type;
    public readonly bool IgnoreAwake = ignoreAwake;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class LockStepEntitySystemOfAttribute(Type type, bool ignoreAwake = false) : BaseAttribute
{
    public readonly Type Type = type;
    public readonly bool IgnoreAwake = ignoreAwake;
}