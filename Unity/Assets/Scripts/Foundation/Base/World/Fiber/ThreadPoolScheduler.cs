using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Chaos
{
    internal sealed class ThreadPoolScheduler : IScheduler
    {
        private bool isDisposed;

        private readonly List<Thread> threads;

        private readonly ConcurrentQueue<Fiber> fiberQueue = new();

        public ThreadPoolScheduler()
        {
            var threadCount = Environment.ProcessorCount;
            threads = new List<Thread>(threadCount);
            for (var i = 0; i < threadCount; ++i)
            {
                Thread thread = new(Update);
                threads.Add(thread);
                thread.Start();
            }
        }

        private void Update()
        {
            var count = 0;
            var threadId = Environment.CurrentManagedThreadId;

            while (true)
            {
                if (count <= 0)
                {
                    Thread.Sleep(1);

                    // count最小为1
                    count = fiberQueue.Count / threads.Count + 1;
                }

                --count;

                if (isDisposed)
                {
                    return;
                }

                if (!fiberQueue.TryDequeue(out var fiber))
                {
                    Thread.Sleep(1);
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

                fiber.ThreadSynchronizationContext.ThreadId = threadId;
                fiber.Update();
                fiber.LateUpdate();
                fiber.ThreadSynchronizationContext.ThreadId = 0;

                fiberQueue.Enqueue(fiber);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            foreach (var thread in threads)
            {
                thread.Join();
            }

            threads.Clear();
        }

        public void AddToScheduler(Fiber fiber)
        {
            fiberQueue.Enqueue(fiber);
        }
    }
}