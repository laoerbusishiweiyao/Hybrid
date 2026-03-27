using System;
using System.Text.Json;
using TMPro;
using UnityEngine;

namespace Chaos
{
    public sealed class WebViewMessageListener : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            var gameObject = new GameObject(nameof(WebViewMessageListener), typeof(WebViewMessageListener));
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 接收 Web 端消息
        /// </summary>
        /// <param name="content"></param>
        public void OnWebMessageReceive(string content)
        {
            var info = JsonSerializer.Deserialize<WebMessageInfo>(content, AppSettings.DefaultJsonSerializerOptions);

            if (info.Payload is not JsonElement element)
            {
                return;
            }

            var opcode = info.Opcode;
            var type = OpcodeRegistry.Default.FindType(opcode);
            var message = element.Deserialize(type, AppSettings.DefaultJsonSerializerOptions);
            switch (message)
            {
                case IWebResponse response:
                {
                    ThreadSynchronizationContext.Default.Post(() => WebSession.Default.OnResponse(response));
                    break;
                }
                case IWebRequest:
                case IWebMessage:
                {
                    ThreadSynchronizationContext.Default.Post(() => WebMessageDispatcher.Default.Handle(opcode, message));
                    break;
                }
            }
        }

        /// <summary>
        /// 接收 Native 端日志
        /// </summary>
        /// <param name="content"></param>
        public void OnNativeLogReceive(string content)
        {
            GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text += content + Environment.NewLine;
        }
    }
}