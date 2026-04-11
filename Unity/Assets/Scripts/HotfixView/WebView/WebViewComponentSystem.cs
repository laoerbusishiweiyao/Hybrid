using System;
using System.Linq;
using System.Text.Json;
using Serilog;
using UnityEngine;

namespace Chaos
{
    [EntitySystemOf(typeof(WebViewComponent))]
    public static partial class WebViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this WebViewComponent self)
        {
            self.Listener = new GameObject(nameof(WebViewMessageListener)).AddComponent<WebViewMessageListener>();
            self.Listener.OnMessageReceived += self.OnListener;

            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            self.CurrentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            var now = TimeInfo.Default.ClientNow();
            self.LastRecvTime = now;
            self.LastSendTime = now;

            self.RequestCallbacks.Clear();
        }

        [EntitySystem]
        private static void Destroy(this WebViewComponent self)
        {
            self.Listener.OnMessageReceived -= self.OnListener;
            UnityEngine.Object.Destroy(self.Listener.gameObject);
            self.Listener = null;

            foreach (var info in self.RequestCallbacks.Values.ToArray())
            {
                info.SetException(new RpcException(StatusCodes.Cancel, $"WebView dispose: {self.Id}"));
            }

            self.RequestCallbacks.Clear();

            self.SendQueue.Clear();
            self.ReceiveQueue.Clear();

            self.Shutdown();

            self.CurrentActivity?.Dispose();
            self.CurrentActivity = null;
        }

        [EntitySystem]
        public static void Update(this WebViewComponent self)
        {
            {
                var count = self.ReceiveQueue.Count;
                while (count-- > 0)
                {
                    if (!self.ReceiveQueue.TryDequeue(out var info))
                    {
                        break;
                    }

                    if (info.Payload is not JsonElement element)
                    {
                        continue;
                    }

                    var type = OpcodeTypeRegistry.Default.GetType(info.Opcode);
                    var message = (MessageObject)element.Deserialize(type, Options.DefaultJsonSerializerOptions);
                    switch (message)
                    {
                        case IWebResponse response:
                        {
                            self.OnResponse(response);
                            break;
                        }
                        case IWebRequest:
                        case IWebMessage:
                        {
                            WebMessageDispatcher.Default.Handle(self.GetParent<WebUiComponent>(), message);
                            break;
                        }
                        default:
                        {
                            Log.Warning("Received unknown message type {type}", type);
                            break;
                        }
                    }
                }
            }

            {
                var count = self.SendQueue.Count;
                while (count-- > 0)
                {
                    if (!self.SendQueue.TryDequeue(out var info))
                    {
                        break;
                    }

                    int opcode = info.Opcode;
                    using var component = new AndroidJavaClass(WebViewComponent.BridgeClassFullName);
                    component.CallStatic("send", self.CurrentActivity, opcode, JsonSerializer.Serialize(info.Message, Options.DefaultJsonSerializerOptions));
                }
            }
        }

        private static void OnListener(this WebViewComponent self, WebMessageInfo info)
        {
            self.ReceiveQueue.Enqueue(info);
        }

        public static void Initialize(this WebViewComponent self, string address = "https://chaos.com", string domain = "chaos.com", string contentPath = "WebUI")
        {
            using var component = new AndroidJavaClass(WebViewComponent.BridgeClassFullName);
            component.CallStatic("load", self.CurrentActivity, address, domain, contentPath);
        }

        public static void Shutdown(this WebViewComponent self)
        {
            using var component = new AndroidJavaClass(WebViewComponent.BridgeClassFullName);
            component.CallStatic("unload", self.CurrentActivity);
            self.CurrentActivity = null;
        }

        public static void OnResponse(this WebViewComponent self, IResponse response)
        {
            WebUiLogger.Default.Recv(self.Fiber(), response);
            self.LastRecvTime = TimeInfo.Default.ClientNow();
            if (!self.RequestCallbacks.TryGetValue(response.RequestId, out var request))
            {
                return;
            }

            request.SetResult(response);
        }

        public static void Send(this WebViewComponent self, IMessage message)
        {
            WebUiLogger.Default.Send(self.Fiber(), message);
            self.LastSendTime = TimeInfo.Default.ClientNow();
            var opcode = OpcodeTypeRegistry.Default.GetOpcode(message.GetType());
            self.SendQueue.Enqueue((opcode, message));
        }

        public static async ThreadTask<IResponse> SendAsync(this WebViewComponent self, IRequest request)
        {
            var requestId = ++self.RequestId;
            var requestInfo = new RequestInfo(request.GetType());
            self.RequestCallbacks[requestId] = requestInfo;

            request.RequestId = requestId;

            self.Send(request);

            void CancelAction()
            {
                if (!self.RequestCallbacks.Remove(requestId, out var info))
                {
                    return;
                }

                var responseType = OpcodeTypeRegistry.Default.GetResponseType(info.RequestType);
                var response = (IResponse)Activator.CreateInstance(responseType);
                response.StatusCode = StatusCodes.Cancel;
                info.SetResult(response);
            }

            var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
            IResponse result;
            try
            {
                cancelSignal?.Add(CancelAction);
                result = await requestInfo.WaitAsync();
            }
            finally
            {
                cancelSignal?.Remove(CancelAction);
            }

            return result;
        }
    }
}