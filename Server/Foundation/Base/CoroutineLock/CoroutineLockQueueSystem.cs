namespace Chaos;

[EntitySystemOf(typeof(CoroutineLockQueue))]
public static partial class CoroutineLockQueueSystem
{
    [EntitySystem]
    private static void Awake(this CoroutineLockQueue self, long type)
    {
        self.Type = type;
        self.RunningCount = 0;
        self.MaxConcurrency = 1;
    }

    [EntitySystem]
    private static void Destroy(this CoroutineLockQueue self)
    {
        self.Queue.Clear();
        self.Type = 0;
        self.MaxConcurrency = 0;
        self.RunningCount = 0;
    }

    internal static async ThreadTask<EntityReference<CoroutineLock>> WaitAsync(this CoroutineLockQueue self, int timeout, int line, string filePath)
    {
        CoroutineLock coroutineLock;
        if (self.RunningCount < self.MaxConcurrency)
        {
            ++self.RunningCount;
            coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.Type, self.Id, 1);
        }
        else
        {
            var task = ThreadTask<EntityReference<CoroutineLock>>.Create(true);
            self.Queue.Enqueue(task);
            coroutineLock = await task;
        }

        coroutineLock.SetTimeoutAsync(timeout, line, filePath).Coroutine();

        return coroutineLock;
    }

    internal static void Notify(this CoroutineLockQueue self, int level)
    {
        --self.RunningCount;

        while (self.Queue.Count > 0)
        {
            if (self.RunningCount >= self.MaxConcurrency)
            {
                break;
            }

            ++self.RunningCount;

            var task = self.Queue.Dequeue();
            var coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.Type, self.Id, level);
            task.SetResult(coroutineLock);
        }
    }
}