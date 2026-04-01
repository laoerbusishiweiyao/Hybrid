using System;
using System.Collections.Generic;

namespace Chaos
{
    public sealed class World : IDisposable
    {
        private static World instance;
        public static World Default => instance ??= new World();

        private readonly SortedDictionary<ushort, HashSet<Singleton>> prioritySingletons = new();
        private readonly Dictionary<Type, Singleton> singletons = new();

        private World()
        {
        }

        public void Dispose()
        {
            instance = null;

            lock (this)
            {
                foreach (var pair in prioritySingletons)
                {
                    foreach (var singleton in pair.Value)
                    {
                        singleton.Dispose();
                    }
                }

                prioritySingletons.Clear();
                singletons.Clear();
            }
        }

        private void AddToOrder(Singleton singleton)
        {
            var priority = singleton.Priority;
            if (!prioritySingletons.TryGetValue(priority, out var set))
            {
                set = new HashSet<Singleton>();
                prioritySingletons[priority] = set;
            }

            set.Add(singleton);
        }

        private void RemoveFromOrder(Singleton singleton)
        {
            var priority = singleton.Priority;
            if (prioritySingletons.TryGetValue(priority, out var set))
            {
                set.Remove(singleton);
            }
        }

        public T AddSingleton<T>() where T : Singleton, ISingletonAwake, new()
        {
            T singleton = new();
            singleton.Awake();

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, TP1>(TP1 p1) where T : Singleton, ISingletonAwake<TP1>, new()
        {
            T singleton = new();
            singleton.Awake(p1);

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, TP1, TP2>(TP1 p1, TP2 p2) where T : Singleton, ISingletonAwake<TP1, TP2>, new()
        {
            T singleton = new();
            singleton.Awake(p1, p2);

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, TP1, TP2, TP3>(TP1 p1, TP2 p2, TP3 p3) where T : Singleton, ISingletonAwake<TP1, TP2, TP3>, new()
        {
            T singleton = new();
            singleton.Awake(p1, p2, p3);

            AddSingleton(singleton);
            return singleton;
        }

        public T AddSingleton<T, TP1, TP2, TP3, TP4>(TP1 p1, TP2 p2, TP3 p3, TP4 p4) where T : Singleton, ISingletonAwake<TP1, TP2, TP3, TP4>, new()
        {
            T singleton = new();
            singleton.Awake(p1, p2, p3, p4);

            AddSingleton(singleton);
            return singleton;
        }

        public void AddSingleton(Singleton singleton)
        {
            lock (this)
            {
                AddToOrder(singleton);
                singletons[singleton.GetType()] = singleton;
            }

            singleton.Register();
        }

        public void ReplaceSingleton(Singleton singleton)
        {
            Singleton oldSingleton;

            lock (this)
            {
                var type = singleton.GetType();
                if (singletons.TryGetValue(type, out oldSingleton))
                {
                    RemoveFromOrder(oldSingleton);
                }

                AddToOrder(singleton);
                singletons[type] = singleton;
            }

            singleton.Register();
            oldSingleton?.Dispose();
        }

        public void RemoveSingleton<T>()
        {
            RemoveSingleton(typeof(T));
        }

        public void RemoveSingleton(Type type)
        {
            Singleton singleton;
            lock (this)
            {
                if (!singletons.Remove(type, out singleton))
                {
                    return;
                }

                RemoveFromOrder(singleton);
            }

            singleton?.Dispose();
        }
    }
}