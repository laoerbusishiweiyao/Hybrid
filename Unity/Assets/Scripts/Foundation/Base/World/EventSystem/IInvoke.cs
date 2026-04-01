using System;

namespace Chaos
{
    public interface IInvoke
    {
        Type EventType { get; }
    }

    public abstract class InvokeHandler<TEventArgs> : HandlerObject, IInvoke where TEventArgs : struct
    {
        public Type EventType => typeof(TEventArgs);

        public abstract void Handle(TEventArgs args);
    }

    public abstract class InvokeHandler<TEventArgs, TResult> : HandlerObject, IInvoke where TEventArgs : struct
    {
        public Type EventType => typeof(TEventArgs);

        public abstract TResult Handle(TEventArgs args);
    }
}