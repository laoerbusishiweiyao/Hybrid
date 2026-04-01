namespace Chaos;

public struct UpdateEventArgs;

public interface IUpdate : IEvent<UpdateEventArgs>;

[EntitySystem]
public abstract class UpdateSystem<TEntity> : EventSystem<TEntity, UpdateEventArgs> where TEntity : Entity, IUpdate
{
    protected override void Execute(TEntity entity, UpdateEventArgs eventArgs)
    {
        Update(entity);
    }

    protected abstract void Update(TEntity self);
}