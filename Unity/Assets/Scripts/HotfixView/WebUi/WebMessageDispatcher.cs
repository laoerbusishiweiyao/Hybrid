using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace Chaos
{
    public struct WebMessageDispatcherInfo
    {
        public readonly int SceneType;
        public readonly IWebMessageHandler Handler;

        public WebMessageDispatcherInfo(int sceneType, IWebMessageHandler handler)
        {
            SceneType = sceneType;
            Handler = handler;
        }
    }

    [CodeProcess]
    public sealed class WebMessageDispatcher : Singleton<WebMessageDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<Type, List<WebMessageDispatcherInfo>> allHandler = new();

        public void Awake()
        {
            var types = CodeTypeRegistry.Default.GetTypes(typeof(WebMessageHandlerAttribute));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is not IWebMessageHandler handler)
                {
                    Log.Error("message handler not inherit IWebMessageHandler abstract class: {type}", type);
                    continue;
                }

                foreach (var attribute in type.GetCustomAttributes<WebMessageHandlerAttribute>(true))
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

                    WebMessageDispatcherInfo info = new(attribute.SceneType, handler);

                    RegisterHandler(messageType, info);
                }
            }
        }

        private void RegisterHandler(Type type, WebMessageDispatcherInfo handler)
        {
            if (!allHandler.ContainsKey(type))
            {
                allHandler.Add(type, new List<WebMessageDispatcherInfo>());
            }

            allHandler[type].Add(handler);
        }

        public void Handle(WebUiComponent session, MessageObject message)
        {
            if (!allHandler.TryGetValue(message.GetType(), out var infos))
            {
                throw new Exception($"not found message handler: {message.GetType()}");
            }

            var sceneType = session.Scene.SceneType;
            foreach (var info in infos)
            {
                if (!SceneTypeMapper.IsSame(info.SceneType, sceneType))
                {
                    continue;
                }

                try
                {
                    info.Handler.Handle(session, message);
                }
                catch (Exception exception)
                {
                    Log.Error("{exception}", exception);
                }
            }
        }
    }
}