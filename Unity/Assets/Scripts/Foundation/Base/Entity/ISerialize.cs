using System;

namespace Chaos
{
    public interface ISerialize
    {
    }

    public interface ISerializeSystem : ISystemType
    {
        void Run(Entity entity);
    }

    [EntitySystem]
    public abstract class SerializeSystem<TEntity> : SystemObject, ISerializeSystem where TEntity : Entity, ISerialize
    {
        void ISerializeSystem.Run(Entity entity)
        {
            Serialize((TEntity)entity);
        }

        Type ISystemType.SystemType => typeof(ISerializeSystem);

        Type ISystemType.EntityType => typeof(TEntity);

        protected abstract void Serialize(TEntity self);
    }
}