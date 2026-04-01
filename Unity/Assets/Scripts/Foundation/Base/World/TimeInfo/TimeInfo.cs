using System;
using System.Diagnostics;

namespace Chaos
{
    public sealed class TimeInfo : Singleton<TimeInfo>, ISingletonAwake
    {
        public int TimeZone { get; set; }

        private DateTime dt1970;
        private long tick1970;

        public long ServerMinusClientTime { private get; set; }

        public void Awake()
        {
            dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            tick1970 = GetTick() - (DateTime.UtcNow.Ticks - dt1970.Ticks) / 10000;
        }

        /// <summary>
        /// 返回毫秒数（跨平台，无状态，超长运行）
        /// </summary>
        private static long GetTick()
        {
            var timestamp = Stopwatch.GetTimestamp();
            return timestamp * 1000 / Stopwatch.Frequency;
        }

        public long ClientNow()
        {
            return GetTick() - tick1970;
        }

        public long ServerNow()
        {
            return ClientNow() + ServerMinusClientTime;
        }
    }
}