namespace Chaos;

public interface IDestroy
{
}

public interface IDestroySystem : ISystemType
{
    void Run(Entity entity);
}

[EntitySystem]
public abstract class DestroySystem<TEntity> : SystemObject, IDestroySystem where TEntity : Entity, IDestroy
{
    void IDestroySystem.Run(Entity entity)
    {
        Destroy((TEntity)entity);
    }

    Type ISystemType.SystemType => typeof(IDestroySystem);

    Type ISystemType.EntityType => typeof(TEntity);

    protected abstract void Destroy(TEntity self);
}