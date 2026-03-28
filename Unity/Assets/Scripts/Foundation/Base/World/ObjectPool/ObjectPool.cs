using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Chaos
{
    public sealed class ObjectPool : Singleton<ObjectPool>, ISingletonAwake
    {
        private readonly ConcurrentDictionary<Type, Pool> typePool = new();

        private readonly Func<Type, Pool> poolFactory = _ => new Pool();

        public void Awake()
        {
        }

        public static T Rent<T>(bool isFromPool = true) where T : class, IPoolable
        {
            return Rent(typeof(T), isFromPool) as T;
        }

        public static object Rent(Type type, bool isFromPool = true)
        {
            if (Default is null || !isFromPool)
            {
                return Activator.CreateInstance(type);
            }

            var pool = Default.GetPool(type);
            var instance = pool.Rent(type);
            if (instance is IPoolable poolable)
            {
                poolable.IsFromPool = true;
            }

            return instance;
        }

        public static void Recycle<T>(ref T instance) where T : class, IPoolable
        {
            Recycle(instance);
            instance = null;
        }

        public static void Recycle(object instance)
        {
            if (Default == null)
            {
                return;
            }

            if (instance is IPoolable { IsFromPool: false })
            {
                return;
            }

            var type = instance.GetType();
            var pool = Default.GetPool(type);
            pool.Return(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Pool GetPool(Type type)
        {
            return typePool.GetOrAdd(type, poolFactory);
        }

#if UNITY_EDITOR
        private sealed class Pool
        {
            private const int MaxCapacity = 1024;

            private readonly System.Collections.Generic.HashSet<object> items = new();

            public object Rent(Type type)
            {
                lock (this)
                {
                    if (items.Count <= 0)
                    {
                        return Activator.CreateInstance(type);
                    }

                    var instance = items.First();
                    items.Remove(instance);
                    return instance;
                }
            }

            public void Return(object instance)
            {
                lock (this)
                {
                    if (items.Count >= MaxCapacity)
                    {
                        return;
                    }

                    if (!items.Add(instance))
                    {
                        throw new Exception("object already in pool: " + instance.GetType().FullName);
                    }
                }
            }
        }
#else
        private sealed class Pool
        {
            private const int MaxCapacity = 1024;

            private int count;
            private readonly ConcurrentQueue<object> queue = new();
            private object fastPathSlot;


            public object Rent(Type type)
            {
                var item = fastPathSlot;
                if (item != null && Interlocked.CompareExchange(ref fastPathSlot, null, item) == item)
                {
                    return item;
                }

                if (!queue.TryDequeue(out item)) return Activator.CreateInstance(type);
                Interlocked.Decrement(ref count);
                return item;
            }

            public void Return(object instance)
            {
                if (fastPathSlot == null && Interlocked.CompareExchange(ref fastPathSlot, instance, null) == null)
                {
                    return;
                }

                if (Interlocked.Increment(ref count) <= MaxCapacity)
                {
                    queue.Enqueue(instance);
                    return;
                }

                Interlocked.Decrement(ref count);
            }
        }
#endif
    }
}