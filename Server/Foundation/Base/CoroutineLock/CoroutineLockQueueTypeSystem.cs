namespace Chaos;

[EntitySystemOf(typeof(CoroutineLockQueueType))]
public static partial class CoroutineLockQueueTypeSystem
{
    [EntitySystem]
    private static void Awake(this CoroutineLockQueueType self)
    {
    }

    private static CoroutineLockQueue Get(this CoroutineLockQueueType self, long key)
    {
        return self.GetChild<CoroutineLockQueue>(key) ?? self.AddChildWithId<CoroutineLockQueue, long>(key, self.Id, true);
    }

    private static void Remove(this CoroutineLockQueueType self, long key)
    {
        self.RemoveChild(key);
    }

    internal static void SetMaxConcurrency(this CoroutineLockQueueType self, long key, int maxConcurrency)
    {
        var coroutineLockQueue = self.Get(key);
        coroutineLockQueue.MaxConcurrency = maxConcurrency;
    }

    internal static async ThreadTask<EntityReference<CoroutineLock>> WaitAsync(this CoroutineLockQueueType self, long key, int timeout, int line, string filePath)
    {
        var queue = self.Get(key);
        return await queue.WaitAsync(timeout, line, filePath);
    }

    internal static void Notify(this CoroutineLockQueueType self, long key, int level)
    {
        var queue = self.Get(key);
        if (queue == null)
        {
            return;
        }

        if (queue.Count == 0)
        {
            self.Remove(key);
            return;
        }

        queue.Notify(level);
    }
}