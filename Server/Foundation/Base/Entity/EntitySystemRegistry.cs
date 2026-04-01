using Serilog;

namespace Chaos;

[CodeProcess]
public sealed class EntitySystemRegistry : Singleton<EntitySystemRegistry>, ISingletonAwake
{
    public readonly TypeSystemGroup TypeSystemGroup = new();

    public void Awake()
    {
        foreach (var type in CodeTypeRegistry.Default.GetTypes(typeof(EntitySystemAttribute)))
        {
            var instance = (SystemObject)Activator.CreateInstance(type);

            if (instance is not ISystemType systemType)
            {
                continue;
            }

            var typeSystemCollection = TypeSystemGroup.FindOrCreate(systemType.EntityType);
            typeSystemCollection.SystemHandlers.Add(systemType.SystemType, instance);
            if (systemType is IEventSystem)
            {
                typeSystemCollection.ClassEventTypes.Add(systemType.SystemType);
            }
        }
    }

    public void Serialize(Entity component)
    {
        if (component is not ISerialize)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(component.GetType(), typeof(ISerializeSystem));
        if (systems == null)
        {
            return;
        }

        foreach (ISerializeSystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(component);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Deserialize(Entity component)
    {
        if (component is not IDeserialize)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(component.GetType(), typeof(IDeserializeSystem));
        if (systems == null)
        {
            return;
        }

        foreach (IDeserializeSystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(component);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void AddComponent(Entity entity, Type type)
    {
        if (entity is not IAddComponentLifespan)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAddComponentLifespanSystem));
        if (systems == null)
        {
            return;
        }

        foreach (IAddComponentLifespanSystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, type);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void GetComponent(Entity entity, Type type)
    {
        if (entity is not IGetComponentLifespan)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IGetComponentLifespanSystem));
        if (systems == null)
        {
            return;
        }

        foreach (IGetComponentLifespanSystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, type);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Awake(Entity entity)
    {
        if (entity is not IAwake)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAwakeSystem));
        if (systems == null)
        {
            return;
        }

        foreach (IAwakeSystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Awake<TP1>(Entity entity, TP1 p1)
    {
        if (entity is not IAwake<TP1>)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAwakeSystem<TP1>));
        if (systems == null)
        {
            return;
        }

        foreach (IAwakeSystem<TP1> system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, p1);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Awake<TP1, TP2>(Entity entity, TP1 p1, TP2 p2)
    {
        if (entity is not IAwake<TP1, TP2>)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAwakeSystem<TP1, TP2>));
        if (systems == null)
        {
            return;
        }

        foreach (IAwakeSystem<TP1, TP2> system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, p1, p2);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Awake<TP1, TP2, TP3>(Entity entity, TP1 p1, TP2 p2, TP3 p3)
    {
        if (entity is not IAwake<TP1, TP2, TP3>)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAwakeSystem<TP1, TP2, TP3>));
        if (systems == null)
        {
            return;
        }

        foreach (IAwakeSystem<TP1, TP2, TP3> system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, p1, p2, p3);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Awake<TP1, TP2, TP3, TP4>(Entity entity, TP1 p1, TP2 p2, TP3 p3, TP4 p4)
    {
        if (entity is not IAwake<TP1, TP2, TP3>)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IAwakeSystem<TP1, TP2, TP3, TP4>));
        if (systems == null)
        {
            return;
        }

        foreach (IAwakeSystem<TP1, TP2, TP3, TP4> system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity, p1, p2, p3, p4);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }

    public void Destroy(Entity entity)
    {
        if (entity is not IDestroy)
        {
            return;
        }

        var systems = TypeSystemGroup.Find(entity.GetType(), typeof(IDestroySystem));
        if (systems == null)
        {
            return;
        }

        foreach (IDestroySystem system in systems)
        {
            if (system == null)
            {
                continue;
            }

            try
            {
                system.Run(entity);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }
}