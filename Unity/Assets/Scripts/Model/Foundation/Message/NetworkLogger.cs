using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chaos
{
    public sealed class NetworkLogger : Singleton<NetworkLogger>, ISingletonAwake
    {
        private readonly HashSet<Type> ignoreTypes = new();

        public void Awake()
        {
        }

        public void AddIgnore(Type type)
        {
            ignoreTypes.Add(type);
        }

        [Conditional("DEBUG")]
        public void Send(Fiber fiber, object msg)
        {
            if (msg is IMessageWrapper wrapper)
            {
                msg = wrapper.MessageObject;
            }

            var type = msg.GetType();
            if (ignoreTypes.Contains(type))
            {
                return;
            }

            fiber.Logger.Debug("Send: {msg}", msg);
        }

        [Conditional("DEBUG")]
        public void Recv(Fiber fiber, object msg)
        {
            if (msg is IMessageWrapper wrapper)
            {
                msg = wrapper.MessageObject;
            }

            var type = msg.GetType();
            if (ignoreTypes.Contains(type))
            {
                return;
            }

            fiber.Logger.Debug("Recv: {msg}", msg);
        }
    }
}