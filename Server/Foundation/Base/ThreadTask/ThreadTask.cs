using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Serilog;

namespace Chaos;

[AsyncMethodBuilder(typeof(ThreadTaskAsyncMethodBuilder))]
public sealed class ThreadTask : ICriticalNotifyCompletion, IThreadTask
{
    public static readonly Action<Exception> ExceptionHandler = exception => Log.Error("{exception}", exception);

    private static ThreadTask completedTask;

    public static ThreadTask CompletedTask => completedTask ??= new ThreadTask { state = AwaiterStatus.Succeeded };

    private static readonly ConcurrentQueue<ThreadTask> queue = new();

    [DebuggerHidden]
    public static ThreadTask Create(bool fromPool = false)
    {
        if (!fromPool)
        {
            return new ThreadTask();
        }

        return !queue.TryDequeue(out var task) ? new ThreadTask { fromPool = true } : task;
    }

    [DebuggerHidden]
    private void Recycle()
    {
        if (!fromPool)
        {
            return;
        }

        state = AwaiterStatus.Pending;
        callback = null;
        Context = null;
        ThreadTaskType = ThreadTaskType.Default;
        // 太多了
        if (queue.Count > 1000)
        {
            return;
        }

        queue.Enqueue(this);
    }

    private bool fromPool;
    private AwaiterStatus state;

    /// <summary>
    /// Action or ExceptionDispatchInfo
    /// </summary>
    private object callback;

    [DebuggerHidden]
    private ThreadTask()
    {
        ThreadTaskType = ThreadTaskType.Default;
    }

    [DebuggerHidden]
    private async ThreadValueTask InnerCoroutine()
    {
        await this;
    }

    [DebuggerHidden]
    public void Coroutine()
    {
        this.SetContext(null);
        InnerCoroutine().Coroutine();
    }

    [DebuggerHidden]
    public void Coroutine(object context)
    {
        this.SetContext(context);
        InnerCoroutine().Coroutine();
    }

    /// <summary>
    /// 在await的同时可以换一个新的上下文
    /// </summary>
    [DebuggerHidden]
    public async ThreadTask NewContext(object context)
    {
        this.SetContext(context);
        await this;
    }

    [DebuggerHidden]
    public ThreadTask GetAwaiter()
    {
        return this;
    }


    public bool IsCompleted
    {
        [DebuggerHidden]
        get => state != AwaiterStatus.Pending;
    }

    [DebuggerHidden]
    public void UnsafeOnCompleted(Action action)
    {
        if (state != AwaiterStatus.Pending)
        {
            action.Invoke();
            return;
        }

        callback = action;
    }

    [DebuggerHidden]
    public void OnCompleted(Action action)
    {
        UnsafeOnCompleted(action);
    }

    [DebuggerHidden]
    public void GetResult()
    {
        switch (state)
        {
            case AwaiterStatus.Succeeded:
                Recycle();
                break;
            case AwaiterStatus.Faulted:
                var exceptionDispatchInfo = callback as ExceptionDispatchInfo;
                callback = null;
                Recycle();
                exceptionDispatchInfo?.Throw();
                break;
            default:
                throw new NotSupportedException("ThreadTask does not allow call GetResult directly when task not completed. Please use 'await'.");
        }
    }

    [DebuggerHidden]
    public void SetResult()
    {
        if (state != AwaiterStatus.Pending)
        {
            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        state = AwaiterStatus.Succeeded;

        var action = callback as Action;
        callback = null;
        action?.Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerHidden]
    public void SetException(Exception exception)
    {
        if (state != AwaiterStatus.Pending)
        {
            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        state = AwaiterStatus.Faulted;

        var action = callback as Action;
        callback = ExceptionDispatchInfo.Capture(exception);
        action?.Invoke();
    }

    public ThreadTaskType ThreadTaskType { get; set; }
    public object Context { get; set; }
}

[AsyncMethodBuilder(typeof(ThreadTaskAsyncMethodBuilder<>))]
public sealed class ThreadTask<T> : ICriticalNotifyCompletion, IThreadTask
{
    private static readonly ConcurrentQueue<ThreadTask<T>> queue = new();

    [DebuggerHidden]
    public static ThreadTask<T> Create(bool fromPool = false)
    {
        if (!fromPool)
        {
            return new ThreadTask<T>();
        }

        return !queue.TryDequeue(out var task) ? new ThreadTask<T> { fromPool = true } : task;
    }

    [DebuggerHidden]
    private void Recycle()
    {
        if (!fromPool)
        {
            return;
        }

        callback = null;
        value = default;
        state = AwaiterStatus.Pending;
        Context = null;
        ThreadTaskType = ThreadTaskType.Default;

        if (queue.Count > 1000)
        {
            return;
        }

        queue.Enqueue(this);
    }

    private bool fromPool;
    private AwaiterStatus state;
    private T value;
    private object callback;

    [DebuggerHidden]
    private ThreadTask()
    {
        ThreadTaskType = ThreadTaskType.Default;
    }

    [DebuggerHidden]
    private async ThreadValueTask InnerCoroutine()
    {
        await this;
    }

    [DebuggerHidden]
    public void Coroutine()
    {
        this.SetContext(null);
        InnerCoroutine().Coroutine();
    }

    [DebuggerHidden]
    public void Coroutine(object context)
    {
        this.SetContext(context);
        InnerCoroutine().Coroutine();
    }

    /// <summary>
    /// 在await的同时可以换一个新的cancellationToken
    /// </summary>
    [DebuggerHidden]
    public async ThreadTask<T> NewContext(object context)
    {
        this.SetContext(context);
        return await this;
    }

    [DebuggerHidden]
    public ThreadTask<T> GetAwaiter()
    {
        return this;
    }

    [DebuggerHidden]
    public T GetResult()
    {
        switch (state)
        {
            case AwaiterStatus.Succeeded:
                var result = value;
                Recycle();
                return result;
            case AwaiterStatus.Faulted:
                var exceptionDispatchInfo = callback as ExceptionDispatchInfo;
                callback = null;
                Recycle();
                exceptionDispatchInfo?.Throw();
                return default;
            default:
                throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
        }
    }

    public bool IsCompleted
    {
        [DebuggerHidden]
        get => state != AwaiterStatus.Pending;
    }

    [DebuggerHidden]
    public void UnsafeOnCompleted(Action action)
    {
        if (state != AwaiterStatus.Pending)
        {
            action.Invoke();
            return;
        }

        callback = action;
    }

    [DebuggerHidden]
    public void OnCompleted(Action action)
    {
        UnsafeOnCompleted(action);
    }

    [DebuggerHidden]
    public void SetResult(T result)
    {
        if (state != AwaiterStatus.Pending)
        {
            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        state = AwaiterStatus.Succeeded;

        value = result;

        var action = callback as Action;
        callback = null;
        action?.Invoke();
    }

    [DebuggerHidden]
    public void SetException(Exception exception)
    {
        if (state != AwaiterStatus.Pending)
        {
            throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
        }

        state = AwaiterStatus.Faulted;

        var action = callback as Action;
        callback = ExceptionDispatchInfo.Capture(exception);
        action?.Invoke();
    }

    public ThreadTaskType ThreadTaskType { get; set; }
    public object Context { get; set; }
}