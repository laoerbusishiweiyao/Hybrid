namespace Chaos
{
    public abstract class Timer<T>: InvokeHandler<TimerCallback> where T: Entity
    {
        public override void Handle(TimerCallback eventArgs)
        {
            this.Run(eventArgs.Args.Entity as T);
        }

        protected abstract void Run(T t);
    }
}