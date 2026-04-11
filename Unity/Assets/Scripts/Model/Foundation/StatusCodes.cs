namespace Chaos
{
    [UniqueId]
    public static partial class StatusCodes
    {
        public const int WithException = 100000000;

        public const int NotFoundActor = WithException + PackageType.Foundation * 1000 + 1;
        public const int RpcFail = WithException + PackageType.Foundation * 1000 + 2;
        public const int MessageTimeout = WithException + PackageType.Foundation * 1000 + 3;
        public const int SessionSendOrRecvTimeout = WithException + PackageType.Foundation * 1000 + 4;
        public const int MessageCountTooMany = WithException + PackageType.Foundation * 1000 + 5;
    }
}