using System.Net;
using Serilog;

namespace Chaos
{
    [EntitySystemOf(typeof(NetComponent))]
    public static partial class NetComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NetComponent self, IKcpTransport kcpTransport)
        {
            self.Service = new KcpService(kcpTransport, ServiceType.External);
            self.Service.AcceptCallback = self.OnAccept;
            self.Service.ReadCallback = self.OnRead;
            self.Service.ErrorCallback = self.OnError;
        }

        [EntitySystem]
        private static void Update(this NetComponent self)
        {
            self.Service.Update();
        }

        [EntitySystem]
        private static void Destroy(this NetComponent self)
        {
            self.Service.Dispose();
        }

        private static void OnError(this NetComponent self, long channelId, int statusCode)
        {
            var session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.StatusCode = statusCode;
            session.Dispose();
        }

        // 只有服务端会accept
        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetComponent self, long channelId, IPEndPoint ipEndPoint)
        {
            var session = self.AddChildWithId<Session, Service>(channelId, self.Service);
            session.RemoteAddress = ipEndPoint;

            // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
            session.AddComponent<SessionAcceptTimeoutComponent>();
            // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
            session.AddComponent<SessionIdleCheckerComponent>();
            session.AddComponent<MessageStatisticsComponent>();
        }

        private static void OnRead(this NetComponent self, long channelId, MemoryBuffer memoryBuffer)
        {
            var session = self.GetChild<Session>(channelId);
            if (session == null)
            {
                return;
            }

            session.LastRecvTime = TimeInfo.Default.ClientNow();

            var (_, message) = MessageSerializer.ToMessage(self.Service, memoryBuffer);
            self.Service.Recycle(memoryBuffer);

            // 外网消息是10000~20000
            var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
            if (opcode is > 20000 or < 10000)
            {
                Log.Error("client message must in (10000, 20000), opcode: {opcode}", opcode);
                return;
            }

            // 机器人消息只在Test进程中处理
            if (message is ITestMessage)
            {
                if (Options.Default.SceneName != "Test")
                {
                    Log.Error("Test message received in non-Test scene: {message}", message.GetType().Name);
                    return;
                }
            }

            NetworkLogger.Default.Recv(self.Fiber(), message);

            EventSystem.Default.Invoke(self.Scene.SceneType, new NetComponentOnReadEventArgs(session, message));
        }

        public static Session Create(this NetComponent self, IPEndPoint realIPEndPoint)
        {
            int channelId;
            while (true)
            {
                channelId = SharedRandom.NextInt32();
                if (self.GetChild<Session>(channelId) == null)
                {
                    break;
                }
            }

            var session = self.AddChildWithId<Session, Service>(channelId, self.Service);
            session.RemoteAddress = realIPEndPoint;
            session.AddComponent<SessionIdleCheckerComponent>();

            self.Service.Create(session.Id, realIPEndPoint);

            return session;
        }

        public static Session Create(this NetComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
        {
            long channelId = localConn;
            var session = self.AddChildWithId<Session, Service>(channelId, self.Service);
            session.RemoteAddress = realIPEndPoint;
            session.AddComponent<SessionIdleCheckerComponent>();
            self.Service.Create(session.Id, routerIPEndPoint);
            return session;
        }

        public static IPEndPoint GetBindPoint(this NetComponent self)
        {
            return self.Service.GetBindPoint();
        }
    }
}