using System.Collections.Generic;

namespace Chaos;

public enum TimerClass
{
    None,
    OnceTimer,
    OnceWaitTimer,
    RepeatedTimer,
}

[ChildOf(typeof(TimerComponent))]
public sealed class TimerAction : Entity, IAwake, IDestroy
{
    public TimerClass TimerClass;

    public int Type;

    public object Object;

    public long StartTime;

    public long Time;
}

public readonly struct TimerCallback
{
    public readonly EntityReference<Entity> Args;

    public TimerCallback(EntityReference<Entity> args)
    {
        Args = args;
    }
}

public sealed partial class Scene
{
    private EntityReference<TimerComponent> timerComponent;

    public TimerComponent TimerComponent
    {
        get => timerComponent;
        set => timerComponent = value;
    }
}

[DisableGetComponent]
[ComponentOf(typeof(Scene))]
public class TimerComponent : Entity, IAwake, IUpdate
{
    /// <summary>
    /// key: 触发时间戳, value: 该时间点待执行的定时器ID列表
    /// </summary>
    public readonly SortedListDictionary<long, long> scheduledTimers = new(1000);

    /// <summary>
    /// 已过期的触发时间戳队列（用于批量处理）
    /// </summary>
    public readonly Queue<long> expiredTimes = new();

    /// <summary>
    /// 已过期的定时器ID队列（与expiredTimes一一对应）
    /// </summary>
    public readonly Queue<long> expiredTimerIds = new();

    /// <summary>
    /// 下一个需要检查的最近触发时间（用于Update中快速跳过）
    /// </summary>
    public long earliestScheduledTime = long.MaxValue;
}