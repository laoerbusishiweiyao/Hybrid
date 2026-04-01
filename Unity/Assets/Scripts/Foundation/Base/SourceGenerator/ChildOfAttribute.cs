using System;

namespace Chaos
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ChildOfAttribute : Attribute
    {
        public readonly Type Type;

        public ChildOfAttribute(Type type = null)
        {
            Type = type;
        }
    }
}