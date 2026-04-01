using System.Runtime.InteropServices;

namespace Chaos
{
    public static class WindowsNativeMethods
    {
        [DllImport("winmm")]
        private static extern void timeBeginPeriod(int period);

        public static void Initialize()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeBeginPeriod(1);
            }
        }
    }
}