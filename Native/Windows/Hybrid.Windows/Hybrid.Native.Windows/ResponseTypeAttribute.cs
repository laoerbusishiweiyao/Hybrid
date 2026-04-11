namespace Hybrid.Native.Windows;

public sealed class ResponseTypeAttribute(string type) : Attribute
{
    public readonly string Type = type;
}