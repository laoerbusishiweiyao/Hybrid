namespace Chaos;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ChildOfAttribute(Type type = null) : Attribute
{
    public readonly Type Type = type;
}