namespace Chaos;

[ChildOf(typeof(CoroutineLockQueueType))]
public sealed class CoroutineLockQueue : Entity, IAwake<long>, IDestroy
{
    internal long Type;

    internal int MaxConcurrency { get; set; }

    internal int RunningCount;

    internal readonly Queue<ThreadTask<EntityReference<CoroutineLock>>> Queue = new();

    internal int Count => Queue.Count;
}