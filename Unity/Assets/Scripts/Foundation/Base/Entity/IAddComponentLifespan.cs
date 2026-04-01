using System;

namespace Chaos
{
    public interface IAddComponentLifespan
    {
    }

    public interface IAddComponentLifespanSystem : ISystemType
    {
        void Run(Entity entity, Type type);
    }

    [EntitySystem]
    public abstract class AddComponentLifespanSystem<TEntity> : SystemObject, IAddComponentLifespanSystem where TEntity : Entity, IAddComponentLifespan
    {
        Type ISystemType.EntityType { get; } = typeof(TEntity);
        Type ISystemType.SystemType { get; } = typeof(IAddComponentLifespanSystem);

        void IAddComponentLifespanSystem.Run(Entity entity, Type type)
        {
            AddComponentLifeSpan((TEntity)entity, type);
        }

        protected abstract void AddComponentLifeSpan(TEntity self, Type type);
    }
}