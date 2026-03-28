using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class HashSetComponent<T> : HashSet<T>, IDisposable
    {
        public static HashSetComponent<T> Create()
        {
            return ObjectPool.Rent(typeof(HashSetComponent<T>)) as HashSetComponent<T>;
        }

        public void Dispose()
        {
            Clear();
            ObjectPool.Recycle(this);
        }
    }
}