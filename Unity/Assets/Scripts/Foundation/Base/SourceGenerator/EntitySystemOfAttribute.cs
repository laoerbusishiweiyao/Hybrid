using System;

namespace Chaos
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntitySystemOfAttribute : BaseAttribute
    {
        public readonly Type Type;
        public readonly bool IgnoreAwake;

        public EntitySystemOfAttribute(Type type, bool ignoreAwake = false)
        {
            Type = type;
            IgnoreAwake = ignoreAwake;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LockStepEntitySystemOfAttribute : BaseAttribute
    {
        public readonly Type Type;
        public readonly bool IgnoreAwake;

        public LockStepEntitySystemOfAttribute(Type type, bool ignoreAwake = false)
        {
            Type = type;
            IgnoreAwake = ignoreAwake;
        }
    }
}