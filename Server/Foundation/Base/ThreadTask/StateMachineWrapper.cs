using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Chaos;

public interface IStateMachineWrapper
{
    Action MoveNext { get; }
    void Recycle();
}

public sealed class StateMachineWrapper<T> : IStateMachineWrapper where T : IAsyncStateMachine
{
    private static readonly ConcurrentQueue<StateMachineWrapper<T>> allWrapper = new();

    public static StateMachineWrapper<T> Fetch(ref T stateMachine)
    {
        if (!allWrapper.TryDequeue(out var wrapper))
        {
            wrapper = new StateMachineWrapper<T>();
        }

        wrapper.stateMachine = stateMachine;
        return wrapper;
    }

    public void Recycle()
    {
        if (allWrapper.Count > 100)
        {
            return;
        }

        stateMachine = default;
        allWrapper.Enqueue(this);
    }

    public Action MoveNext { get; }

    private T stateMachine;

    private StateMachineWrapper()
    {
        MoveNext = Run;
    }

    private void Run()
    {
        stateMachine?.MoveNext();
    }
}