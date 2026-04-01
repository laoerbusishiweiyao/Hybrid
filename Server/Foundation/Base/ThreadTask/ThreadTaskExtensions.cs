namespace Chaos;

internal static class ThreadTaskExtensions
{
    internal static void SetContext(this IThreadTask task, object context)
    {
        while (true)
        {
            if (task.ThreadTaskType == ThreadTaskType.ContextTask)
            {
                ((ThreadTask<object>)task).SetResult(context);
                break;
            }

            // cancellationToken传下去
            task.ThreadTaskType = ThreadTaskType.WithContext;
            var child = task.Context;
            task.Context = context;
            task = child as IThreadTask;
            if (task == null)
            {
                break;
            }

            // 传递到WithContext为止，因为可能这一层设置了新的context
            if (task.ThreadTaskType == ThreadTaskType.WithContext)
            {
                break;
            }
        }
    }
}

public sealed partial class ThreadTask
{
    public static async ThreadTask<T> GetContextAsync<T>() where T : class
    {
        var task = ThreadTask<object>.Create(true);
        task.ThreadTaskType = ThreadTaskType.ContextTask;
        var result = await task;
        return result as T;
    }

    public static async ThreadTask<object> GetContextAsync()
    {
        var task = ThreadTask<object>.Create(true);
        task.ThreadTaskType = ThreadTaskType.ContextTask;
        var result = await task;
        return result;
    }

    private sealed class CoroutineBlocker
    {
        private int count;

        private ThreadTask tcs;

        public CoroutineBlocker(int count)
        {
            this.count = count;
        }

        public async ThreadTask RunSubCoroutineAsync(ThreadTask task)
        {
            try
            {
                await task;
            }
            finally
            {
                --count;

                if (count <= 0 && tcs != null)
                {
                    var threadTask = tcs;
                    tcs = null;
                    threadTask.SetResult();
                }
            }
        }

        public async ThreadTask WaitAsync()
        {
            if (count <= 0)
            {
                return;
            }

            tcs = Create(true);
            await tcs;
        }
    }

    public static async ThreadTask WaitAnyAsync(List<ThreadTask> tasks)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        var context = await GetContextAsync();

        CoroutineBlocker coroutineBlocker = new(1);

        foreach (var task in tasks)
        {
            coroutineBlocker.RunSubCoroutineAsync(task).Coroutine(context);
        }

        await coroutineBlocker.WaitAsync();
    }

    public static async ThreadTask WaitAnyAsync(ThreadTask[] tasks)
    {
        if (tasks.Length == 0)
        {
            return;
        }

        var context = await GetContextAsync();
        CoroutineBlocker coroutineBlocker = new(1);

        foreach (var task in tasks)
        {
            coroutineBlocker.RunSubCoroutineAsync(task).Coroutine(context);
        }

        await coroutineBlocker.WaitAsync();
    }

    public static async ThreadTask WaitAllAsync(ThreadTask[] tasks)
    {
        if (tasks.Length == 0)
        {
            return;
        }

        var context = await GetContextAsync();
        CoroutineBlocker coroutineBlocker = new(tasks.Length);

        foreach (var task in tasks)
        {
            coroutineBlocker.RunSubCoroutineAsync(task).Coroutine(context);
        }

        await coroutineBlocker.WaitAsync();
    }

    public static async ThreadTask WaitAllAsync(List<ThreadTask> tasks)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        var context = await GetContextAsync();
        CoroutineBlocker coroutineBlocker = new(tasks.Count);

        foreach (var task in tasks)
        {
            coroutineBlocker.RunSubCoroutineAsync(task).Coroutine(context);
        }

        await coroutineBlocker.WaitAsync();
    }
}