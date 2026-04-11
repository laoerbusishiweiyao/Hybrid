namespace Chaos
{
    [UniqueId]
    public static partial class TimerInvokeType
    {
        public const int SessionAcceptTimeout = PackageType.Foundation * 1000 + 1;
        public const int SessionIdleChecker = PackageType.Foundation * 1000 + 2;

        public const int TestOnceTimer = PackageType.Foundation * 1000 + 100;
        public const int TestRepeatedTimer = PackageType.Foundation * 1000 + 101;
    }
}