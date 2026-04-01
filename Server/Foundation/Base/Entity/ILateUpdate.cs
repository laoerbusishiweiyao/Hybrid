namespace Chaos;

public struct LateUpdateEventArgs
{
}

public interface ILateUpdate : IEvent<LateUpdateEventArgs>
{
}

[EntitySystem]
public abstract class LateUpdateSystem<TEntity> : EventSystem<TEntity, LateUpdateEventArgs> where TEntity : Entity, ILateUpdate
{
    protected override void Execute(TEntity entity, LateUpdateEventArgs eventArgs)
    {
        LateUpdate(entity);
    }

    protected abstract void LateUpdate(TEntity self);
}