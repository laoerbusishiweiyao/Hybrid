namespace Chaos;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ComponentOfAttribute(Type type = null) : Attribute
{
    public readonly Type Type = type;
}