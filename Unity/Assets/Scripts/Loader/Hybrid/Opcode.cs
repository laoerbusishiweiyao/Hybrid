namespace Chaos
{
    public static class Opcode
    {
        #region 内部保留

        public const ushort Web = 102;

        public const ushort WebLoaded = Web * 100 + 1;
        public const ushort WebTouchData = Web * 100 + 2;
        public const ushort WebMouseData = Web * 100 + 3;


        public const ushort WebWindowShutdown = 10300;

        #endregion

        public const ushort UnityInformationRequest = 10301;
        public const ushort UnityInformationResponse = 10302;

        public const ushort UnityInitialized = 10401;

        public const ushort BrowserInformationRequest = 10402;
        public const ushort BrowserInformationResponse = 10403;
    }

    public sealed record UnityInitialized : MessageObject, IWebMessage
    {
    }

    public sealed record BrowserInformationRequest : MessageObject, IWebRequest
    {
        public int RequestId { get; set; }
    }

    public sealed record BrowserInformationResponse : MessageObject, IWebResponse
    {
        public int RequestId { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public string UserAgent { get; set; }
    }
}