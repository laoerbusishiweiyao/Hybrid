using System.Reflection;
using Serilog;

namespace Chaos;

[CodeProcess]
public sealed class EventSystem : Singleton<EventSystem>, ISingletonAwake
{
    private class EventInfo
    {
        public IEvent Event { get; }

        public int SceneType { get; }

        public EventInfo(IEvent @event, int sceneType)
        {
            Event = @event;
            SceneType = sceneType;
        }
    }

    private readonly Dictionary<Type, List<EventInfo>> allEvents = new();

    private readonly Dictionary<Type, Dictionary<long, object>> allInvokers = new();

    public void Awake()
    {
        foreach (var type in CodeTypeRegistry.Default.GetTypes(typeof(EventHandlerAttribute)))
        {
            if (Activator.CreateInstance(type) is not IEvent instance)
            {
                throw new Exception($"type not is IEvent: {type.Name}");
            }

            foreach (var attribute in type.GetCustomAttributes<EventHandlerAttribute>(false))
            {
                var eventType = instance.EventType;
                EventInfo info = new(instance, attribute.SceneType);
                if (!allEvents.ContainsKey(eventType))
                {
                    allEvents.Add(eventType, new List<EventInfo>());
                }

                allEvents[eventType].Add(info);
            }
        }

        foreach (var type in CodeTypeRegistry.Default.GetTypes(typeof(InvokeHandlerAttribute)))
        {
            if (Activator.CreateInstance(type) is not IInvoke instance)
            {
                throw new Exception($"type not is callback: {type.Name}");
            }

            foreach (var attribute in type.GetCustomAttributes<InvokeHandlerAttribute>(false))
            {
                var eventType = instance.EventType;
                if (!allInvokers.TryGetValue(eventType, out var dictionary))
                {
                    dictionary = new Dictionary<long, object>();
                    allInvokers.Add(eventType, dictionary);
                }

                try
                {
                    dictionary.Add(attribute.Type, instance);
                }
                catch (Exception exception)
                {
                    throw new Exception($"action type duplicate: {instance.EventType.Name} {attribute.Type}", exception);
                }
            }
        }
    }

    public async ThreadTask PublishAsync<TScene, TEventArgs>(TScene scene, TEventArgs eventArgs) where TScene : class, IScene where TEventArgs : struct
    {
        if (!allEvents.TryGetValue(typeof(TEventArgs), out var eventInfos))
        {
            return;
        }

        using var list = ListComponent<ThreadTask>.Create();

        var sceneType = scene.SceneType;
        foreach (var eventInfo in eventInfos)
        {
            if (!SceneTypeMapper.IsSame(sceneType, eventInfo.SceneType))
            {
                continue;
            }

            if (eventInfo.Event is not EventHandler<TScene, TEventArgs> handler)
            {
                Log.Error("event error: {type}", eventInfo.Event.GetType().FullName);
                continue;
            }

            list.Add(handler.Handle(scene, eventArgs));
        }

        try
        {
            await ThreadTask.WaitAllAsync(list);
        }
        catch (Exception exception)
        {
            Log.Error("{exception}", exception);
        }
    }

    public void Publish<TScene, TEventArgs>(TScene scene, TEventArgs eventArgs) where TScene : class, IScene where TEventArgs : struct
    {
        if (!allEvents.TryGetValue(typeof(TEventArgs), out var eventInfos))
        {
            return;
        }

        var sceneType = scene.SceneType;
        foreach (var eventInfo in eventInfos)
        {
            if (!SceneTypeMapper.IsSame(sceneType, eventInfo.SceneType))
            {
                continue;
            }


            if (eventInfo.Event is not EventHandler<TScene, TEventArgs> handler)
            {
                Log.Error("event error: {type}", eventInfo.Event.GetType().FullName);
                continue;
            }

            handler.Handle(scene, eventArgs).Coroutine();
        }
    }

    public TInvoker GetInvoker<TInvoker, TEventArgs>(long type) where TInvoker : class, IInvoke where TEventArgs : struct
    {
        if (!allInvokers.TryGetValue(typeof(TEventArgs), out var invokeHandlers))
        {
            return null;
        }

        if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
        {
            return null;
        }

        if (invokeHandler is not TInvoker invoker)
        {
            return null;
        }

        return invoker;
    }

    public List<long> GetAllInvokerType<TEventArgs>() where TEventArgs : struct
    {
        return !allInvokers.TryGetValue(typeof(TEventArgs), out var invokeHandlers) ? new List<long>() : invokeHandlers.Keys.ToList();
    }

    public void Invoke<TEventArgs>(long type, TEventArgs eventArgs) where TEventArgs : struct
    {
        var invoker = GetInvoker<InvokeHandler<TEventArgs>, TEventArgs>(type);
        if (invoker == null)
        {
            throw new Exception($"Invoke error, not AInvokeHandler: {type} {typeof(TEventArgs).FullName}");
        }

        invoker.Handle(eventArgs);
    }

    public TResult Invoke<TEventArgs, TResult>(long type, TEventArgs eventArgs) where TEventArgs : struct
    {
        var invoker = GetInvoker<InvokeHandler<TEventArgs, TResult>, TEventArgs>(type);
        if (invoker == null)
        {
            throw new Exception($"Invoke error, not AInvokeHandler: {type} {typeof(TEventArgs).FullName}");
        }

        return invoker.Handle(eventArgs);
    }

    public void Invoke<TEventArgs>(TEventArgs eventArgs) where TEventArgs : struct
    {
        Invoke(0, eventArgs);
    }

    public TResult Invoke<TEventArgs, TResult>(TEventArgs eventArgs) where TEventArgs : struct
    {
        return Invoke<TEventArgs, TResult>(0, eventArgs);
    }

    public void TryInvoke<TEventArgs>(long type, TEventArgs eventArgs) where TEventArgs : struct
    {
        var invoker = GetInvoker<InvokeHandler<TEventArgs>, TEventArgs>(type);
        invoker?.Handle(eventArgs);
    }

    public TResult TryInvoke<TEventArgs, TResult>(long type, TEventArgs eventArgs) where TEventArgs : struct
    {
        var invoker = GetInvoker<InvokeHandler<TEventArgs, TResult>, TEventArgs>(type);
        return invoker == null ? default : invoker.Handle(eventArgs);
    }

    public void TryInvoke<TEventArgs>(TEventArgs eventArgs) where TEventArgs : struct
    {
        TryInvoke(0, eventArgs);
    }

    public TResult TryInvoke<TEventArgs, TResult>(TEventArgs eventArgs) where TEventArgs : struct
    {
        return TryInvoke<TEventArgs, TResult>(0, eventArgs);
    }
}