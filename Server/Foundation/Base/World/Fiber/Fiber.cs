using Serilog;

namespace Chaos;

public readonly struct FiberDestroyEventArgs;

public sealed class Fiber : IScheduler
{
    [ThreadStatic]
    private static Fiber instance;

    internal static Fiber Default
    {
        get => instance;
        set
        {
            if (value == null)
            {
                return;
            }

            instance = value;
            SynchronizationContext.SetSynchronizationContext(instance.ThreadSynchronizationContext);
        }
    }

    public bool IsDisposed { get; private set; }

    public int Id { get; }

    public int Zone { get; }

    private EntityReference<Scene> root;

    public SchedulerType SchedulerType { get; }

    public int ParentFiberId { get; }

    private FiberMonitorInfo fiberMonitorInfo;

    private int instanceIdGenerator = 1;

    public int NewInstanceId()
    {
        return instanceIdGenerator++;
    }

    public Scene Root
    {
        get => root;
        private set => root = value;
    }

    public string Name { get; }

    public EntitySystem EntitySystem { get; }
    public Mailboxes Mailboxes { get; private set; }
    public ThreadSynchronizationContext ThreadSynchronizationContext { get; }
    public ILogger Logger { get; }

    private readonly Queue<ThreadTask> frameFinishTasks = new();

    private readonly Queue<Fiber> schedulerQueue = new();

    private readonly Dictionary<int, Fiber> children = new();

    internal Fiber(int id, long rootId, int zone, int sceneType, string name, SchedulerType schedulerType, Fiber parent)
    {
        Id = id;
        Zone = zone;
        SchedulerType = schedulerType;
        EntitySystem = new EntitySystem();
        Mailboxes = new Mailboxes();
        ParentFiberId = parent?.Id ?? 0;
        Name = name;

        if (schedulerType == SchedulerType.Parent)
        {
            Logger = parent?.Logger;
            ThreadSynchronizationContext = parent?.ThreadSynchronizationContext;
        }
        else
        {
            ThreadSynchronizationContext = new ThreadSynchronizationContext();
            Logger = EventSystem.Default.Invoke<LoggerBuildEventArgs, ILogger>(new LoggerBuildEventArgs(name)).ForContext("Scene", name);
        }

        Root = new Scene(this, rootId, sceneType, name);
    }

    internal void Update()
    {
        try
        {
            Default = this;

            fiberMonitorInfo = new FiberMonitorInfo(Name);

            var start = Environment.TickCount;

            EntitySystem.Publish(new UpdateEventArgs());

            var end = Environment.TickCount;
            fiberMonitorInfo.UpdateTimeUsed = end - start;

            var count = schedulerQueue.Count;
            while (count-- > 0)
            {
                var fiber = schedulerQueue.Dequeue();

                if (fiber.IsDisposed)
                {
                    children.Remove(fiber.Id);
                    continue;
                }

                schedulerQueue.Enqueue(fiber);

                fiber.Update();
            }
        }
        catch (Exception exception)
        {
            Logger.Error("{exception}", exception);
        }
    }

    internal void LateUpdate()
    {
        try
        {
            Default = this;

            var start = Environment.TickCount;

            EntitySystem.Publish(new LateUpdateEventArgs());
            FrameFinishUpdate();
            ThreadSynchronizationContext.Update();

            var end = Environment.TickCount;
            fiberMonitorInfo.LateUpdateTimeUsed = end - start;

            FiberRegistry.Default?.AddMonitor(Id, ref fiberMonitorInfo);

            var count = schedulerQueue.Count;
            while (count-- > 0)
            {
                var fiber = schedulerQueue.Dequeue();

                if (fiber.IsDisposed)
                {
                    children.Remove(fiber.Id);
                    continue;
                }

                schedulerQueue.Enqueue(fiber);

                fiber.LateUpdate();
            }
        }
        catch (Exception exception)
        {
            Logger.Error("{exception}", exception);
        }
    }

    public async ThreadTask WaitFrameFinishAsync()
    {
        var task = ThreadTask.Create(true);
        frameFinishTasks.Enqueue(task);
        await task;
    }

    private void FrameFinishUpdate()
    {
        while (frameFinishTasks.Count > 0)
        {
            var task = frameFinishTasks.Dequeue();
            task.SetResult();
        }
    }

    /// <summary>
    /// 创建的fiber在由该fiber调度，所以可以返回Fiber，父Fiber可以直接操作子Fiber
    /// </summary>
    /// <param name="rootId"></param>
    /// <param name="zone"></param>
    /// <param name="sceneType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async ThreadTask<Fiber> CreateFiber(long rootId, int zone, int sceneType, string name)
    {
        var fiber = await FiberRegistry.Default.CreateFiberAsync(SchedulerType.Parent, rootId, zone, sceneType, name, this);
        children.Add(fiber.Id, fiber);
        return fiber;
    }

    /// <summary>
    /// 这个会跟parent fiber在同一线程调度，所以可以返回Fiber，父Fiber可以直接操作子Fiber
    /// </summary>
    /// <param name="fiberId"></param>
    /// <param name="rootId"></param>
    /// <param name="zone"></param>
    /// <param name="sceneType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async ThreadTask<Fiber> CreateFiberWithId(int fiberId, long rootId, int zone, int sceneType, string name)
    {
        var fiber = await FiberRegistry.Default.CreateFiberAsync(fiberId, SchedulerType.Parent, rootId, zone, sceneType, name, this);
        children.Add(fiber.Id, fiber);
        return fiber;
    }

    public async ThreadTask<int> CreateFiber(SchedulerType schedulerType, long rootId, int zone, int sceneType, string name)
    {
        if (Options.Default.SingleThread == 1)
        {
            schedulerType = SchedulerType.Parent;
        }

        var fiber = await FiberRegistry.Default.CreateFiberAsync(schedulerType, rootId, zone, sceneType, name, this);
        children.Add(fiber.Id, fiber);
        return fiber.Id;
    }

    public async ThreadTask<int> CreateFiberWithId(int fiberId, SchedulerType schedulerType, long rootId, int zone, int sceneType, string name)
    {
        if (Options.Default.SingleThread == 1)
        {
            schedulerType = SchedulerType.Parent;
        }

        var fiber = await FiberRegistry.Default.CreateFiberAsync(fiberId, schedulerType, rootId, zone, sceneType, name, this);
        children.Add(fiber.Id, fiber);
        return fiber.Id;
    }

    public async ThreadTask RemoveFiber(int fiberId)
    {
        if (!children.Remove(fiberId, out var fiber))
        {
            return;
        }

        IScheduler scheduler = fiber;
        if (FiberRegistry.Default == null)
        {
            scheduler.Dispose();
            return;
        }

        if (fiber.SchedulerType == SchedulerType.Parent)
        {
            foreach (var child in fiber.children.Keys.ToArray())
            {
                await fiber.RemoveFiber(child);
            }

            await EventSystem.Default.PublishAsync(fiber.Root, new FiberDestroyEventArgs());
            scheduler.Dispose();
            return;
        }

        TaskCompletionSource<bool> taskCompletionSource = new();
        fiber.ThreadSynchronizationContext.Post(() =>
        {
            FiberDestroy().Coroutine();
            return;

            async ThreadTask FiberDestroy()
            {
                foreach (var child in fiber.children.Keys.ToArray())
                {
                    await fiber.RemoveFiber(child);
                }

                var fiberRoot = (scheduler as Fiber)?.Root;
                await EventSystem.Default.PublishAsync(fiberRoot, new FiberDestroyEventArgs());
                scheduler.Dispose();
                taskCompletionSource.SetResult(true);
            }
        });
        await taskCompletionSource.Task;
    }

    public async ThreadTask RemoveFibers()
    {
        foreach (var key in children.Keys.ToArray())
        {
            await RemoveFiber(key);
        }
    }

    public Fiber GetFiber(int id)
    {
        if (!children.TryGetValue(id, out var fiber))
        {
            return null;
        }

        return fiber.SchedulerType != SchedulerType.Parent ? null : fiber;
    }

    public Fiber GetFiber(string name)
    {
        foreach (var fiber in children.Values)
        {
            if (fiber.SchedulerType != SchedulerType.Parent)
            {
                continue;
            }

            if (fiber.Name != name)
            {
                continue;
            }

            return fiber;
        }

        return null;
    }

    void IDisposable.Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        try
        {
            Root.Dispose();
        }
        catch (Exception exception)
        {
            Logger.Error("{exception}", exception);
        }

        FiberRegistry.Default?.RemoveMonitor(Id);

        foreach (var child in children.Keys.ToArray())
        {
            RemoveFiber(child).Coroutine();
        }
    }

    public void AddToScheduler(Fiber fiber)
    {
        schedulerQueue.Enqueue(fiber);
    }
}