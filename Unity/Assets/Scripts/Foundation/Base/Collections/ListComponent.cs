using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class ListComponent<T> : List<T>, IDisposable
    {
        public static ListComponent<T> Create()
        {
            return ObjectPool.Rent(typeof(ListComponent<T>)) as ListComponent<T>;
        }

        public void Dispose()
        {
            if (Capacity > 64)
            {
                return;
            }

            Clear();
            ObjectPool.Recycle(this);
        }
    }
}