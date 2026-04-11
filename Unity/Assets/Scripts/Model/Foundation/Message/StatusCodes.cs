namespace Chaos
{
    public static partial class StatusCodes
    {
        public const int Success = 0;

        public const int WithoutException = 200000000;

        public const int Cancel = 200000000 + PackageType.Foundation * 1000 + 1;
        public const int Timeout = 200000000 + PackageType.Foundation * 1000 + 2;

        public static bool IsNeedThrowException(int statusCode)
        {
            return statusCode switch
            {
                0 or > WithoutException => false,
                _ => true
            };
        }
    }
}