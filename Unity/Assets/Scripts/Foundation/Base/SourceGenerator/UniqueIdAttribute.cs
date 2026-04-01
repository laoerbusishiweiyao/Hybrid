using System;

namespace Chaos
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UniqueIdAttribute : Attribute
    {
        public readonly int Min;

        public readonly int Max;

        public UniqueIdAttribute(int min = int.MinValue, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }
    }
}