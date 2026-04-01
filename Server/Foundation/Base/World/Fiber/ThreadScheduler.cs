using System.Collections.Concurrent;

namespace Chaos;

internal sealed class ThreadScheduler : IScheduler
{
    private bool isDisposed;

    private readonly ConcurrentDictionary<int, Thread> dictionary = new();

    private void Update(Fiber fiber)
    {
        var fiberId = fiber.Id;
        while (true)
        {
            if (isDisposed)
            {
                return;
            }

            if (fiber.IsDisposed)
            {
                dictionary.Remove(fiberId, out _);
                return;
            }

            fiber.Update();
            fiber.LateUpdate();

            Thread.Sleep(1);
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        foreach (var pair in dictionary.ToArray())
        {
            pair.Value.Join();
        }

        dictionary.Clear();
    }

    public void AddToScheduler(Fiber fiber)
    {
        Thread thread = new(() => Update(fiber));

        fiber.ThreadSynchronizationContext.ThreadId = thread.ManagedThreadId;

        dictionary.TryAdd(fiber.Id, thread);
        thread.Start();
    }
}