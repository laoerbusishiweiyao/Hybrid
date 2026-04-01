using System;

namespace Chaos;

[EntitySystemOf(typeof(TimerAction))]
public static partial class TimerActionSystem
{
    [EntitySystem]
    private static void Awake(this TimerAction self)
    {
    }

    [EntitySystem]
    private static void Destroy(this TimerAction self)
    {
        self.TimerClass = TimerClass.None;
        self.StartTime = 0;
        self.Time = 0;
        self.Type = 0;

        if (self.Object is IDisposable disposable)
        {
            disposable.Dispose();
        }

        self.Object = null;
    }

    internal static Entity GetEntity(this TimerAction self)
    {
        var wrapper = (ValueTypeWrapper<EntityReference<Entity>>)self.Object;
        return wrapper.Value;
    }
}

[EntitySystemOf(typeof(TimerComponent))]
public static partial class TimerComponentSystem
{
    [EntitySystem]
    private static void Awake(this TimerComponent self)
    {
        self.GetParent<Scene>().TimerComponent = self;
    }

    [EntitySystem]
    private static void Update(this TimerComponent self)
    {
        if (self.scheduledTimers.Count == 0)
        {
            return;
        }

        var now = self.GetNow();

        if (now < self.earliestScheduledTime)
        {
            return;
        }

        foreach (var pair in self.scheduledTimers)
        {
            var scheduledTime = pair.Key;
            if (scheduledTime > now)
            {
                self.earliestScheduledTime = scheduledTime;
                break;
            }

            self.expiredTimes.Enqueue(scheduledTime);
        }

        while (self.expiredTimes.Count > 0)
        {
            var expiredTime = self.expiredTimes.Dequeue();
            var scheduledTimerIds = self.scheduledTimers[expiredTime];
            foreach (var timerId in scheduledTimerIds)
            {
                self.expiredTimerIds.Enqueue(timerId);
            }

            self.scheduledTimers.Remove(expiredTime);
        }

        if (self.scheduledTimers.Count == 0)
        {
            self.earliestScheduledTime = long.MaxValue;
        }

        while (self.expiredTimerIds.Count > 0)
        {
            var expiredTimerId = self.expiredTimerIds.Dequeue();
            self.Run(expiredTimerId);
        }
    }

    private static long GetNow(this TimerComponent self)
    {
        return TimeInfo.Default.ServerNow();
    }

    private static void Run(this TimerComponent self, long expiredTimerId)
    {
        var timerAction = self.GetChild<TimerAction>(expiredTimerId);
        if (timerAction == null)
        {
            return;
        }

        switch (timerAction.TimerClass)
        {
            case TimerClass.OnceTimer:
            {
                var entity = timerAction.GetEntity();
                var timerActionType = timerAction.Type;
                self.RemoveChild(expiredTimerId);

                if (entity != null)
                {
                    EventSystem.Default.Invoke(timerActionType, new TimerCallback(entity));
                }

                break;
            }
            case TimerClass.OnceWaitTimer:
            {
                var task = (ThreadTask)timerAction.Object;
                self.RemoveChild(expiredTimerId);
                task.SetResult();
                break;
            }
            case TimerClass.RepeatedTimer:
            {
                var timerActionType = timerAction.Type;
                var entity = timerAction.GetEntity();

                if (entity != null)
                {
                    timerAction.StartTime = self.GetNow();
                    self.AddTimer(timerAction);
                    EventSystem.Default.Invoke(timerActionType, new TimerCallback(entity));
                }
                else
                {
                    self.RemoveChild(expiredTimerId);
                }

                break;
            }
        }
    }

    private static TimerAction CreateTimerAction(this TimerComponent self, TimerClass timerClass, long startTime, long time, int type, object args)
    {
        var timer = self.AddChild<TimerAction>(true);
        timer.TimerClass = timerClass;
        timer.StartTime = startTime;
        timer.Object = args;
        timer.Time = time;
        timer.Type = type;

        self.AddTimer(timer);
        return timer;
    }

    private static void AddTimer(this TimerComponent self, TimerAction timerAction)
    {
        var tillTime = timerAction.StartTime + timerAction.Time;
        self.scheduledTimers.Add(tillTime, timerAction.Id);
        if (tillTime < self.earliestScheduledTime)
        {
            self.earliestScheduledTime = tillTime;
        }
    }

    public static bool Remove(this TimerComponent self, ref long id)
    {
        var cache = id;
        id = 0;
        return self.Remove(cache);
    }

    private static bool Remove(this TimerComponent self, long id)
    {
        return id != 0 && self.RemoveChild(id);
    }

    public static async ThreadTask WaitTillAsync(this TimerComponent self, long tillTime)
    {
        var now = self.GetNow();
        if (now >= tillTime)
        {
            return;
        }

        var task = ThreadTask.Create(true);
        var timerAction = self.CreateTimerAction(TimerClass.OnceWaitTimer, now, tillTime - now, 0, task);
        var timerActionId = timerAction.Id;

        var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
        try
        {
            cancelSignal?.Add(CancelAction);
            await task;
        }
        finally
        {
            cancelSignal?.Remove(CancelAction);
        }

        return;

        void CancelAction()
        {
            if (!self.Remove(timerActionId))
            {
                return;
            }

            task.SetResult();
        }
    }

    public static async ThreadTask WaitFrameAsync(this TimerComponent self)
    {
        await self.WaitAsync(1);
    }

    public static async ThreadTask WaitAsync(this TimerComponent self, long duration)
    {
        if (duration == 0)
        {
            return;
        }

        var now = self.GetNow();

        var task = ThreadTask.Create(true);
        var timerAction = self.CreateTimerAction(TimerClass.OnceWaitTimer, now, duration, 0, task);
        var timerActionId = timerAction.Id;

        var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
        try
        {
            cancelSignal?.Add(CancelAction);
            await task;
        }
        finally
        {
            cancelSignal?.Remove(CancelAction);
        }

        return;

        void CancelAction()
        {
            if (!self.Remove(timerActionId))
            {
                return;
            }

            task.SetResult();
        }
    }

    public static long NewOnceTimer(this TimerComponent self, long tillTime, int type, Entity args)
    {
        var timeNow = self.GetNow();

        EntityReference<Entity> reference = args;
        var wrapper = ValueTypeWrapper<EntityReference<Entity>>.Create(reference);
        var timerAction = self.CreateTimerAction(TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, wrapper);
        return timerAction.Id;
    }

    public static long NewFrameTimer(this TimerComponent self, int type, Entity args)
    {
        return self.NewRepeatedTimerInner(100, type, args);
    }

    private static long NewRepeatedTimerInner(this TimerComponent self, long time, int type, Entity args)
    {
        if (time < 50)
        {
            throw new Exception($"repeated timer < 50, timerType: time: {time}");
        }

        var now = self.GetNow();
        EntityReference<Entity> reference = args;
        var wrapper = ValueTypeWrapper<EntityReference<Entity>>.Create(reference);
        var timerAction = self.CreateTimerAction(TimerClass.RepeatedTimer, now, time, type, wrapper);
        return timerAction.Id;
    }

    public static long NewRepeatedTimer(this TimerComponent self, long time, int type, Entity args)
    {
        return self.NewRepeatedTimerInner(time, type, args);
    }
}