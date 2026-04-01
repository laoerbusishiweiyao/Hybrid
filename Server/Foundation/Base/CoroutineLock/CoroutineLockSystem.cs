using Serilog;

namespace Chaos;

[EntitySystemOf(typeof(CoroutineLock))]
public static partial class CoroutineLockSystem
{
    [EntitySystem]
    private static void Awake(this CoroutineLock self, long type, long key, int count)
    {
        self.Type = type;
        self.Key = key;
        self.Level = count;
    }

    [EntitySystem]
    private static void Destroy(this CoroutineLock self)
    {
        self.Scene<CoroutineLockComponent>().RunNextCoroutine(self.Type, self.Key, self.Level + 1);
        self.Type = 0;
        self.Key = 0;
        self.Level = 0;
    }

    internal static async ThreadTask SetTimeoutAsync(this CoroutineLock self, int timeout, int line, string filePath)
    {
        EntityReference<CoroutineLock> reference = self;
        var type = self.Type;
        var key = self.Key;
        var level = self.Level;

        var timerComponent = self.Root().TimerComponent;
        await timerComponent.WaitAsync(timeout);

        self = reference;

        if (self != null)
        {
            Log.Error("{filePath}:{line} Coroutine lock timeout after {timeout}ms, type: {type}, key: {key}, level: {level}", filePath, line, timeout, type, key, level);
            reference.Dispose();
        }
    }
}