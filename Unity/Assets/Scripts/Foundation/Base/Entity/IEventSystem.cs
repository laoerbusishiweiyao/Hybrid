using System;

namespace Chaos
{
    public interface IEvent<TEventArgs> where TEventArgs : struct
    {
    }

    public interface IEventSystem
    {
    }

    public interface IEventSystem<in TEventArgs> : ISystemType, IEventSystem where TEventArgs : struct
    {
        void Run(Entity entity, TEventArgs eventArgs);
    }

    public abstract class EventSystem<TEntity, TEventArgs> : SystemObject, IEventSystem<TEventArgs> where TEntity : Entity, IEvent<TEventArgs> where TEventArgs : struct
    {
        public void Run(Entity entity, TEventArgs eventArgs)
        {
            Execute((TEntity)entity, eventArgs);
        }

        public Type EntityType => typeof(TEntity);
        public Type SystemType => typeof(IEventSystem<TEventArgs>);

        protected abstract void Execute(TEntity entity, TEventArgs eventArgs);
    }
}