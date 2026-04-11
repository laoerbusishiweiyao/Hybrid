using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog;

namespace Chaos
{
    public readonly struct SessionMessageDispatcherInfo
    {
        public readonly int SceneType;
        public readonly ISessionMessageHandler Handler;

        public SessionMessageDispatcherInfo(int sceneType, ISessionMessageHandler handler)
        {
            SceneType = sceneType;
            Handler = handler;
        }
    }

    [CodeProcess]
    public sealed class SessionMessageDispatcher : Singleton<SessionMessageDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<ushort, List<SessionMessageDispatcherInfo>> allHandler = new();

        public void Awake()
        {
            var types = CodeTypeRegistry.Default.GetTypes(typeof(SessionMessageHandlerAttribute));

            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is not ISessionMessageHandler handler)
                {
                    Log.Error("session message handler type {type} is not inherit ISessionMessageHandler", type);
                    continue;
                }

                foreach (var attribute in type.GetCustomAttributes<SessionMessageHandlerAttribute>(false))
                {
                    var messageType = handler.MessageType;
                    var opcode = OpcodeTypeRegistry.Default.GetOpcode(messageType);
                    if (opcode is 0)
                    {
                        Log.Error("message {type} 's opcode is 0", messageType);
                        continue;
                    }

                    var info = new SessionMessageDispatcherInfo(attribute.SceneType, handler);
                    RegisterHandler(opcode, info);
                }
            }
        }

        private void RegisterHandler(ushort opcode, SessionMessageDispatcherInfo info)
        {
            if (!allHandler.ContainsKey(opcode))
            {
                allHandler.Add(opcode, new List<SessionMessageDispatcherInfo>());
            }

            allHandler[opcode].Add(info);
        }

        public void Handle(Session session, object message)
        {
            var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
            if (!allHandler.TryGetValue(opcode, out var infos))
            {
                Log.Error("message {opcode} = {message} has no handlers", opcode, message);
                return;
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