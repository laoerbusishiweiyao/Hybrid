using System;

namespace Chaos
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ComponentOfAttribute : Attribute
    {
        public readonly Type Type;

        public ComponentOfAttribute(Type type = null)
        {
            Type = type;
        }
    }
}