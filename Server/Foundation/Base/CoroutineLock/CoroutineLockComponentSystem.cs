using System.Runtime.CompilerServices;
using Serilog;

namespace Chaos;

[EntitySystemOf(typeof(CoroutineLockComponent))]
public static partial class CoroutineLockComponentSystem
{
    [EntitySystem]
    private static void Awake(this CoroutineLockComponent self)
    {
        self.GetParent<Scene>().CoroutineLockComponent = self;
    }

    [EntitySystem]
    private static void Update(this CoroutineLockComponent self)
    {
        var count = self.DeferredContinuations.Count;
        for (var i = 0; i < count; i++)
        {
            var (coroutineLockType, key, level) = self.DeferredContinuations.Dequeue();
            self.Notify(coroutineLockType, key, level);
        }
    }

    internal static void RunNextCoroutine(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
    {
        if (level == 100)
        {
            Log.Warning("too much coroutine level: {coroutineLockType} {key}", coroutineLockType, key);
        }

        self.DeferredContinuations.Enqueue((coroutineLockType, key, level));
    }

    private static CoroutineLockQueueType Get(this CoroutineLockComponent self, long coroutineLockType)
    {
        return self.GetChild<CoroutineLockQueueType>(coroutineLockType) ?? self.AddChildWithId<CoroutineLockQueueType>(coroutineLockType);
    }

    /// <summary>
    /// 控制并发执行数量
    /// </summary>
    public static void SetMaxConcurrency(this CoroutineLockComponent self, long coroutineLockType, long key, int maxConcurrency)
    {
        var coroutineLockQueueType = self.Get(coroutineLockType);
        coroutineLockQueueType.SetMaxConcurrency(key, maxConcurrency);
    }

    /// <summary>
    /// 等待协程锁，带超时参数
    /// </summary>
    /// <param name="self">协程锁组件</param>
    /// <param name="coroutineLockType">锁类型</param>
    /// <param name="key">锁键值</param>
    /// <param name="timeout">协程锁占用时间，超时则自动释放</param>
    /// <param name="line"></param>
    /// <param name="filePath"></param>
    /// <returns>协程锁引用</returns>
    public static async ThreadTask<EntityReference<CoroutineLock>> Wait(this CoroutineLockComponent self, long coroutineLockType, long key, int timeout = 30000, [CallerLineNumber] int line = 0, [CallerFilePath] string filePath = "")
    {
        if (timeout <= 0) // 必须要计时，防止死锁
        {
            timeout = 30000;
        }

        var coroutineLockQueueType = self.Get(coroutineLockType);
        return await coroutineLockQueueType.WaitAsync(key, timeout, line, filePath);
    }

    private static void Notify(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
    {
        var coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType);
        coroutineLockQueueType?.Notify(key, level);
    }
}