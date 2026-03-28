using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace Chaos
{
    public struct ThreadTaskAsyncMethodBuilder
    {
        private IStateMachineWrapper wrapper;

        private ThreadTask task;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ThreadTaskAsyncMethodBuilder Create()
        {
            ThreadTaskAsyncMethodBuilder builder = new() { task = ThreadTask.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden] public ThreadTask Task => task;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (wrapper != null)
            {
                wrapper.Recycle();
                wrapper = null;
            }

            task.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            if (wrapper != null)
            {
                wrapper.Recycle();
                wrapper = null;
            }

            task.SetResult();
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : class, IThreadTask, INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(wrapper.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(wrapper.MoveNext);

            if (awaiter is not IThreadTask threadTask)
            {
                return;
            }

            if (task.ThreadTaskType == ThreadTaskType.WithContext)
            {
                threadTask.SetContext(task.Context);
                return;
            }

            task.Context = threadTask;
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct ThreadTaskAsyncMethodBuilder<T>
    {
        private IStateMachineWrapper wrapper;

        private ThreadTask<T> task;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ThreadTaskAsyncMethodBuilder<T> Create()
        {
            ThreadTaskAsyncMethodBuilder<T> builder = new ThreadTaskAsyncMethodBuilder<T>() { task = ThreadTask<T>.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden] public ThreadTask<T> Task => task;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (wrapper != null)
            {
                wrapper.Recycle();
                wrapper = null;
            }

            task.SetException(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult(T result)
        {
            if (wrapper != null)
            {
                wrapper.Recycle();
                wrapper = null;
            }

            task.SetResult(result);
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(wrapper.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(wrapper.MoveNext);

            if (awaiter is not IThreadTask threadTask)
            {
                return;
            }

            if (task.ThreadTaskType == ThreadTaskType.WithContext)
            {
                threadTask.SetContext(task.Context);
                return;
            }

            task.Context = threadTask;
        }

        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}