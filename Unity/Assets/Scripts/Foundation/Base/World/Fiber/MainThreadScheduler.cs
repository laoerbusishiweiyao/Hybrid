using System;
using System.Collections.Concurrent;

namespace Chaos
{
    internal sealed class MainThreadScheduler : IScheduler
    {
        private readonly ConcurrentQueue<Fiber> fiberQueue = new();
        private readonly ConcurrentQueue<Fiber> addQueue = new();

        private readonly int threadId = Environment.CurrentManagedThreadId;

        public void Dispose()
        {
            addQueue.Clear();
            fiberQueue.Clear();
        }

        public void Update()
        {
            var count = fiberQueue.Count;
            while (count-- > 0)
            {
                if (!fiberQueue.TryDequeue(out var fiber))
                {
                    continue;
                }

                if (fiber == null)
                {
                    continue;
                }

                if (fiber.IsDisposed)
                {
                    continue;
                }

                fiberQueue.Enqueue(fiber);

                fiber.Update();
            }
        }

        public void LateUpdate()
        {
            var count = fiberQueue.Count;
            while (count-- > 0)
            {
                if (!fiberQueue.TryDequeue(out var fiber))
                {
                    continue;
                }

                if (fiber == null)
                {
                    continue;
                }

                if (fiber.IsDisposed)
                {
                    continue;
                }

                fiberQueue.Enqueue(fiber);

                fiber.LateUpdate();
            }

            while (addQueue.Count > 0)
            {
                addQueue.TryDequeue(out var fiber);
                fiberQueue.Enqueue(fiber);
            }
        }


        public void AddToScheduler(Fiber fiber)
        {
            fiber.ThreadSynchronizationContext.ThreadId = threadId;
            addQueue.Enqueue(fiber);
        }
    }
}