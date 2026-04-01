namespace Chaos;

public interface IGetComponentLifespan
{
}

public interface IGetComponentLifespanSystem : ISystemType
{
    void Run(Entity entity, Type type);
}

[EntitySystem]
public abstract class GetComponentLifespanSystem<TEntity> : SystemObject, IGetComponentLifespanSystem where TEntity : Entity, IGetComponentLifespan
{
    Type ISystemType.EntityType { get; } = typeof(TEntity);
    Type ISystemType.SystemType { get; } = typeof(IGetComponentLifespanSystem);

    void IGetComponentLifespanSystem.Run(Entity entity, Type type)
    {
        GetComponentLifeSpan((TEntity)entity, type);
    }

    protected abstract void GetComponentLifeSpan(TEntity self, Type type);
}