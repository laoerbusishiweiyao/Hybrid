using System;

namespace Chaos
{
    public interface IDeserialize
    {
    }

    public interface IDeserializeSystem : ISystemType
    {
        void Run(Entity entity);
    }

    [EntitySystem]
    public abstract class DeserializeSystem<TEntity> : SystemObject, IDeserializeSystem where TEntity : Entity, IDeserialize
    {
        void IDeserializeSystem.Run(Entity entity)
        {
            this.Deserialize((TEntity)entity);
        }

        Type ISystemType.SystemType => typeof(IDeserializeSystem);

        Type ISystemType.EntityType => typeof(TEntity);

        protected abstract void Deserialize(TEntity self);
    }
}