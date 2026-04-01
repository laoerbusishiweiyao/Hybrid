namespace Chaos;

public abstract class Timer<T>: InvokeHandler<TimerCallback> where T: Entity
{
    public override void Handle(TimerCallback a)
    {
        this.Run(a.Args.Entity as T);
    }

    protected abstract void Run(T t);
}