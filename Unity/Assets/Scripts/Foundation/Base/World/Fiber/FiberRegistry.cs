using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Chaos
{
    public enum SchedulerType
    {
        Parent = -1,
        Main = 0,
        Thread = 1,
        ThreadPool = 2,
    }

    public struct FiberMonitorInfo
    {
        public string Name;

        public int UpdateTimeUsed;

        public int LateUpdateTimeUsed;

        public FiberMonitorInfo(string name)
        {
            Name = name;
            UpdateTimeUsed = 0;
            LateUpdateTimeUsed = 0;
        }
    }

    public sealed class FiberRegistry : Singleton<FiberRegistry>, ISingletonAwake
    {
        private int idGenerator;

        private readonly IScheduler[] schedulers = new IScheduler[3];

        private MainThreadScheduler mainThreadScheduler;

        private Fiber mainFiber;

        private readonly ThreadSynchronizationContext context = new();

        private readonly ConcurrentDictionary<int, FiberMonitorInfo> fiberMonitorInfos = new();

        public void AddMonitor(int fiberId, ref FiberMonitorInfo fiberMonitorInfo)
        {
        }

        public void RemoveMonitor(int fiberId)
        {
        }

        public void Awake()
        {
            SynchronizationContext.SetSynchronizationContext(context);

            mainThreadScheduler = new MainThreadScheduler();
            schedulers[(int)SchedulerType.Main] = mainThreadScheduler;

            if (Options.Default.SingleThread == 1)
            {
                schedulers[(int)SchedulerType.Thread] = mainThreadScheduler;
                schedulers[(int)SchedulerType.ThreadPool] = mainThreadScheduler;
            }
            else
            {
                schedulers[(int)SchedulerType.Thread] = new ThreadScheduler();
                schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler();
            }
        }

        public override ushort Priority => 0;

        public void Update()
        {
            context.Update();

            mainThreadScheduler.Update();

            // Unity 回调
            Fiber.Default = mainFiber;
        }

        public void LateUpdate()
        {
            mainThreadScheduler.LateUpdate();

            // Unity 回调
            Fiber.Default = mainFiber;
        }

        protected override void Destroy()
        {
            foreach (var scheduler in schedulers)
            {
                scheduler.Dispose();
            }

            (mainFiber as IScheduler)?.Dispose();
        }

        public async ThreadTask<int> CreateMainFiberAsync(int sceneType, string sceneName)
        {
            if (mainFiber != null)
            {
                throw new Exception("FiberManager is already created");
            }

            mainFiber = await CreateFiberAsync(SchedulerType.Main, IdGenerator.Default.GenerateId(), 0, sceneType, sceneName, null);
            return mainFiber.Id;
        }

        /// <summary>
        /// 创建纤程
        /// </summary>
        /// <param name="schedulerType">纤程调度器，-1: 主线程，-2: 当前线程，-3: 线程池, 其它: 纤程</param>
        /// <param name="fiberId">纤程id</param>
        /// <param name="rootId"></param>
        /// <param name="zone">区</param>
        /// <param name="sceneType">场景类型</param>
        /// <param name="name">纤程名称</param>
        /// <param name="parent"></param>
        internal async ThreadTask<Fiber> CreateFiberAsync(int fiberId, SchedulerType schedulerType, long rootId, int zone, int sceneType, string name, Fiber parent)
        {
            if (sceneType == 0)
            {
                throw new Exception("fiberId is 0");
            }

            try
            {
                var parentId = parent?.Id ?? 0;
                Log.Debug("Create Fiber: {name} {fiberId} {zone} {sceneType} {schedulerType} {parentId}", name, fiberId, zone, sceneType, schedulerType, parentId);

                // 如果调度器是父fiber，那么日志也是父fiber的日志
                Fiber fiber = new(fiberId, rootId, zone, sceneType, name, schedulerType, parent);

                var scheduler = schedulerType == SchedulerType.Parent ? parent : schedulers[(int)schedulerType];
                scheduler?.AddToScheduler(fiber);

                TaskCompletionSource<bool> task = new();

                fiber.ThreadSynchronizationContext.Post(() => { Action().Coroutine(); });
                await task.Task;

                Log.Debug("Create Fiber {name} finish", name);
                return fiber;

                async ThreadTask Action()
                {
                    try
                    {
                        await EventSystem.Default.Invoke<FiberInitializeEventArgs, ThreadTask>(sceneType, new FiberInitializeEventArgs(fiber));
                        task.SetResult(true);
                    }
                    catch (Exception exception)
                    {
                        task.SetException(new Exception($"Initialize Fiber Fail: {sceneType}", exception));
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Create Fiber Error: {fiberId} {sceneType}", exception);
            }
        }

        private int GetFiberId()
        {
            return Interlocked.Increment(ref idGenerator);
        }

        /// <summary>
        /// 创建纤程
        /// </summary>
        /// <param name="schedulerType">纤程调度器，-1: 主线程，-2: 当前线程，-3: 线程池, 其它: 纤程</param>
        /// <param name="rootId"></param>
        /// <param name="zone">区</param>
        /// <param name="sceneType">场景类型</param>
        /// <param name="name">纤程名称</param>
        /// <param name="parent"></param>
        /// <returns>纤程id</returns>
        internal async ThreadTask<Fiber> CreateFiberAsync(SchedulerType schedulerType, long rootId, int zone, int sceneType, string name, Fiber parent)
        {
            var fiberId = GetFiberId();
            return await CreateFiberAsync(fiberId, schedulerType, rootId, zone, sceneType, name, parent);
        }
    }
}