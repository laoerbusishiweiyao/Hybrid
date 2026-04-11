using System;
using Serilog;

namespace Chaos
{
    [EntitySystemOf(typeof(SessionAcceptTimeoutComponent))]
    public static partial class SessionAcceptTimeoutComponentSystem
    {
        [InvokeHandler(TimerInvokeType.SessionAcceptTimeout)]
        public sealed class SessionAcceptTimeout : Timer<SessionAcceptTimeoutComponent>
        {
            protected override void Run(SessionAcceptTimeoutComponent self)
            {
                try
                {
                    self.Parent.Dispose();
                }
                catch (Exception exception)
                {
                    Log.Error("move timer error: {self}\n{exception}", self.Id, exception);
                }
            }
        }

        [EntitySystem]
        private static void Awake(this SessionAcceptTimeoutComponent self)
        {
            self.Timer = self.Root().TimerComponent.NewOnceTimer(TimeInfo.Default.ServerNow() + 5000, TimerInvokeType.SessionAcceptTimeout, self);
        }

        [EntitySystem]
        private static void Destroy(this SessionAcceptTimeoutComponent self)
        {
            self.Root().TimerComponent?.Remove(ref self.Timer);
        }
    }
}