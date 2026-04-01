namespace Chaos
{
    [UniqueId]
    internal static partial class StatusCodes
    {
        public const int Success = 0;

        public const int UserStatusCode = 110000;

        public const int KcpConnectTimeout = 100205;
        public const int KcpAcceptTimeout = 100206;
        public const int KcpReadWriteTimeout = 100207;
        public const int PeerDisconnect = 100208;
        public const int SocketCantSend = 100209;
        public const int SocketError = 100210;
        public const int KcpWaitSendSizeTooLarge = 100211;
        public const int KcpCreateError = 100212;
        public const int SendMessageNotFoundTChannel = 100213;
        public const int TChannelRecvError = 100214;
        public const int MessageSocketParserError = 100215;
        public const int KcpNotFoundChannel = 100216;

        public const int WebsocketSendError = 100217;
        public const int WebsocketPeerReset = 100218;
        public const int WebsocketMessageTooBig = 100219;
        public const int WebsocketRecvError = 100220;

        public const int KcpReadNotSame = 100230;
        public const int KcpSplitError = 100231;
        public const int KcpSplitCountError = 100232;

        public const int PacketParserError = 110005;
        public const int WebsocketConnectError = 110304;

        public const int WithException = 100000000;
    }
}