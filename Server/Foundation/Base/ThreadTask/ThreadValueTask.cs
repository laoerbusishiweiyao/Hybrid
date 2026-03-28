using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chaos;

[AsyncMethodBuilder(typeof(ThreadValueTaskAsyncMethodBuilder))]
internal struct ThreadValueTask : ICriticalNotifyCompletion
{
    [DebuggerHidden]
    public void Coroutine()
    {
    }

    [DebuggerHidden]
    public bool IsCompleted => true;

    [DebuggerHidden]
    public void OnCompleted(Action continuation)
    {
    }

    [DebuggerHidden]
    public void UnsafeOnCompleted(Action continuation)
    {
    }
}