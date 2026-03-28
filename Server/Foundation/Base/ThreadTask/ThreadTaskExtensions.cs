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