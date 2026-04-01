using System.Diagnostics.CodeAnalysis;
using Windows.Win32;

namespace Chaos;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class WindowsNativeMethods
{
    public static void Initialize()
    {
        if (OperatingSystem.IsWindows())
        {
            PInvoke.timeBeginPeriod(1);
        }
    }
}