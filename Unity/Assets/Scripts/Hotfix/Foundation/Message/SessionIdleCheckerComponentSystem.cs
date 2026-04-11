using System;
using Serilog;

namespace Chaos
{
    [EntitySystemOf(typeof(SessionIdleCheckerComponent))]
    public static partial class SessionIdleCheckerComponentSystem
    {
        [InvokeHandler(TimerInvokeType.SessionIdleChecker)]
        public sealed class SessionIdleChecker : Timer<SessionIdleCheckerComponent>
        {
            protected override void Run(SessionIdleCheckerComponent self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception exception)
                {
                    Log.Error("session idle checker timer error: {self}\n{exception}", self.Id, exception);
                }
            }
        }

        [EntitySystem]
        private static void Awake(this SessionIdleCheckerComponent self)
        {
            self.RepeatedTimer = self.Root().TimerComponent.NewRepeatedTimer(CheckInterval, TimerInvokeType.SessionIdleChecker, self);
        }

        [EntitySystem]
        private static void Destroy(this SessionIdleCheckerComponent self)
        {
            var root = self.Root();
            root?.TimerComponent?.Remove(ref self.RepeatedTimer);
        }

        private const int CheckInterval = 2000;

#if UNITY_EDITOR || DEBUG
        public const int SessionTimeoutTime = 400000;
#else
        public const int SessionTimeoutTime = 40000;
#endif

        private static void Check(this SessionIdleCheckerComponent self)
        {
            var session = self.GetParent<Session>();
            var timeNow = TimeInfo.Default.ClientNow();

            if (timeNow - session.LastRecvTime < SessionTimeoutTime && timeNow - session.LastSendTime < SessionTimeoutTime)
            {
                return;
            }

            Log.Information("session timeout: {SessionId} {TimeNow} {SessionLastRecvTime} {SessionLastSendTime} {LastRecvTime} {LastSendTime}", session.Id, timeNow, session.LastRecvTime, session.LastSendTime, timeNow - session.LastRecvTime, timeNow - session.LastSendTime);
            session.StatusCode = StatusCodes.SessionSendOrRecvTimeout;

            session.Dispose();
        }
    }
}