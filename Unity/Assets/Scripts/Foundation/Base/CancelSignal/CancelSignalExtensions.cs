using System;

namespace Chaos
{
    public static class CancelSignalExtensions
    {
        public static bool IsCancel(this CancelSignal self)
        {
            if (self == null)
            {
                return false;
            }

            return self.IsDisposed;
        }

        private static async ThreadTask TimeoutAsync(this CancelSignal self, long afterTime)
        {
            if (afterTime <= 0)
            {
                return;
            }

            if (self.IsCancel())
            {
                return;
            }

            await Fiber.Default.Root.TimerComponent.WaitAsync(afterTime);
            if (self.IsCancel())
            {
                return;
            }

            self.Cancel();
        }

        public static async ThreadTask AddCancel(this ThreadTask task, CancelSignal cancelSignal)
        {
            if (cancelSignal == null)
            {
                throw new Exception("add cancel token is null");
            }

            var signal = await ThreadTask.GetContextAsync<CancelSignal>();

            signal?.Add(cancelSignal.Cancel);

            await task.NewContext(cancelSignal);
        }

        /// <summary>
        /// 增加一个canceltoken，可以用新增的canceltoken取消协程，当然也可以用老的取消
        /// </summary>
        public static async ThreadTask<T> AddCancel<T>(this ThreadTask<T> task, CancelSignal cancelSignal)
        {
            if (cancelSignal == null)
            {
                throw new Exception("add cancel token is null");
            }

            var signal = await ThreadTask.GetContextAsync<CancelSignal>();

            signal?.Add(cancelSignal.Cancel);

            return await task.NewContext(cancelSignal);
        }

        public static async ThreadTask TimeoutAsync(this ThreadTask task, CancelSignal cancelSignal, long afterTime)
        {
            cancelSignal.TimeoutAsync(afterTime).Coroutine();
            await AddCancel(task, cancelSignal);
        }

        public static async ThreadTask<T> TimeoutAsync<T>(this ThreadTask<T> task, CancelSignal cancelSignal, long afterTime)
        {
            cancelSignal.TimeoutAsync(afterTime).Coroutine();
            return await AddCancel(task, cancelSignal);
        }

        public static async ThreadTask TimeoutAsync(this ThreadTask task, long afterTime)
        {
            CancelSignal cancellationToken = new();
            cancellationToken.TimeoutAsync(afterTime).Coroutine();
            await AddCancel(task, cancellationToken);
        }

        public static async ThreadTask<T> TimeoutAsync<T>(this ThreadTask<T> task, long afterTime)
        {
            CancelSignal cancellationToken = new();
            cancellationToken.TimeoutAsync(afterTime).Coroutine();
            return await AddCancel(task, cancellationToken);
        }
    }
}