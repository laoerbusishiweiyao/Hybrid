using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Chaos
{
    [ComponentOf(typeof(WebUiComponent))]
    public sealed class WebViewComponent : Entity, IAwake, IDestroy, IUpdate
    {
        public const string BridgeClassFullName = "com.chaosstudio.nativebridge.WebViewComponent";

        public AndroidJavaObject CurrentActivity { get; set; }

        public readonly ConcurrentQueue<(ushort Opcode, IMessage Message)> SendQueue = new();

        public readonly ConcurrentQueue<WebMessageInfo> ReceiveQueue = new();

        public WebViewMessageListener Listener;

        public int RequestId { get; set; }

        public readonly Dictionary<int, RequestInfo> RequestCallbacks = new();

        public long LastRecvTime { get; set; }

        public long LastSendTime { get; set; }
    }
}