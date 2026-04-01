using System;
using System.Collections.Generic;
using Serilog;

namespace Chaos
{
    public class EntitySystem
    {
        private readonly Dictionary<Type, Queue<EntityReference<Entity>>> queues = new();

        public Queue<EntityReference<Entity>> GetQueue(Type type)
        {
            if (!queues.TryGetValue(type, out var queue))
            {
                queue = new Queue<EntityReference<Entity>>();
                queues.Add(type, queue);
            }

            return queue;
        }

        public virtual void RegisterSystem(Entity component)
        {
            var type = component.GetType();

            var typeSystemCollection = EntitySystemRegistry.Default.TypeSystemGroup.Find(type);
            if (typeSystemCollection == null)
            {
                return;
            }

            foreach (var queueType in typeSystemCollection.ClassEventTypes)
            {
                var queue = GetQueue(queueType);
                queue.Enqueue(component);
            }
        }

        public void Publish<TEventArgs>(TEventArgs eventArgs) where TEventArgs : struct
        {
            var systemType = typeof(IEventSystem<TEventArgs>);
            var queue = GetQueue(systemType);
            var count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                if (component is not IEvent<TEventArgs>)
                {
                    continue;
                }

                try
                {
                    var systems = EntitySystemRegistry.Default.TypeSystemGroup.Find(component.GetType(), systemType);
                    if (systems == null)
                    {
                        continue;
                    }

                    queue.Enqueue(component);

                    foreach (IEventSystem<TEventArgs> system in systems)
                    {
                        try
                        {
                            system.Run(component, eventArgs);
                        }
                        catch (Exception exception)
                        {
                            Log.Error("{exception}", exception);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", exception);
                }
            }
        }
    }
}