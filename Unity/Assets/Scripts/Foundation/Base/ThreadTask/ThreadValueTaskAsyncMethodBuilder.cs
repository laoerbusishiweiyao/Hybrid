using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace Chaos
{
    internal struct ThreadValueTaskAsyncMethodBuilder
    {
        private IStateMachineWrapper wrapper;

        // 1. Static Create method.
        [DebuggerHidden]
        public static ThreadValueTaskAsyncMethodBuilder Create()
        {
            ThreadValueTaskAsyncMethodBuilder builder = new();
            return builder;
        }

        // 2. TaskLike Task property(void)
        [DebuggerHidden] public ThreadValueTask Task => default;

        // 3. SetException
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (wrapper != null)
            {
                wrapper.Recycle();
                wrapper = null;
            }

            ThreadTask.ExceptionHandler.Invoke(exception);
        }

        // 4. SetResult
        [DebuggerHidden]
        public void SetResult()
        {
            if (wrapper == null)
            {
                return;
            }

            wrapper.Recycle();
            wrapper = null;
        }

        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : IThreadTask, INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(wrapper.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : IThreadTask, ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            wrapper ??= StateMachineWrapper<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(wrapper.MoveNext);
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