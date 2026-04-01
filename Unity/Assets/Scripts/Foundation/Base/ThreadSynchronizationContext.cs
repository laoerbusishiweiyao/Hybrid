using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace Chaos
{
    public sealed class ThreadSynchronizationContext : SynchronizationContext
    {
        public static readonly ThreadSynchronizationContext Default = new();

        internal int ThreadId { get; set; }

        private readonly ConcurrentQueue<Action> callbacks = new();

        private Action callback;

        public void Update()
        {
            while (true)
            {
                if (!callbacks.TryDequeue(out callback))
                {
                    return;
                }

                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Debug.Log(exception);
                }
            }
        }

        public override void Post(SendOrPostCallback action, object state)
        {
            Post(() => action(state));
        }

        public void Post(Action action)
        {
            callbacks.Enqueue(action);
        }
    }
}