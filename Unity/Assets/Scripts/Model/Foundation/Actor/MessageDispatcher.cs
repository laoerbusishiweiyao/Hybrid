using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace Chaos
{
    public readonly struct MessageDispatcherInfo
    {
        public readonly int SceneType;
        public readonly IMessageHandler Handler;

        public MessageDispatcherInfo(int sceneType, IMessageHandler handler)
        {
            SceneType = sceneType;
            Handler = handler;
        }
    }

    [CodeProcess]
    public sealed class MessageDispatcher : Singleton<MessageDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<Type, List<MessageDispatcherInfo>> allHandler = new();

        public void Awake()
        {
            var types = CodeTypeRegistry.Default.GetTypes(typeof(MessageHandlerAttribute));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is not IMessageHandler handler)
                {
                    Log.Error("message handler not inherit IMessageHandler abstract class: {type}", type);
                    continue;
                }

                foreach (var messageHandlerAttribute in type.GetCustomAttributes<MessageHandlerAttribute>(true))
                {
                    var messageType = handler.RequestType;

                    var responseType = handler.ResponseType;
                    if (responseType != null)
                    {
                        if (OpcodeTypeRegistry.Default.GetResponseType(messageType) != responseType)
                        {
                            throw new InvalidOperationException($"message handler response type {messageType} not supported");
                        }
                    }

                    MessageDispatcherInfo info = new(messageHandlerAttribute.SceneType, handler);

                    RegisterHandler(messageType, info);
                }
            }
        }

        private void RegisterHandler(Type type, MessageDispatcherInfo handler)
        {
            if (!allHandler.ContainsKey(type))
            {
                allHandler.Add(type, new List<MessageDispatcherInfo>());
            }

            allHandler[type].Add(handler);
        }

        public async ThreadTask HandleAsync(Entity entity, int fromFiber, MessageObject message)
        {
            if (!allHandler.TryGetValue(message.GetType(), out var infos))
            {
                throw new Exception($"not found message handler: {message} {entity.GetType().FullName}");
            }

            EntityReference<Entity> reference = entity;
            var sceneType = entity.Scene.SceneType;
            foreach (var info in infos)
            {
                if (!SceneTypeMapper.IsSame(info.SceneType, sceneType))
                {
                    continue;
                }

                entity = reference;
                await info.Handler.Handle(entity, fromFiber, message);
            }
        }

        public void Handle(Entity entity, int fromFiber, MessageObject message)
        {
            if (!allHandler.TryGetValue(message.GetType(), out var infos))
            {
                throw new Exception($"not found message handler: {message} {entity.GetType().FullName}");
            }

            var sceneType = entity.Scene.SceneType;
            foreach (var info in infos)
            {
                if (!SceneTypeMapper.IsSame(info.SceneType, sceneType))
                {
                    continue;
                }

                info.Handler.Handle(entity, fromFiber, message).Coroutine();
            }
        }
    }
}