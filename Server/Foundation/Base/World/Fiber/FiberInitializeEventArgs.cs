namespace Chaos;

public readonly struct FiberInitializeEventArgs
{
    public readonly Fiber Fiber;

    public FiberInitializeEventArgs(Fiber fiber)
    {
        Fiber = fiber;
    }
}