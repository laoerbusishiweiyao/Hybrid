using System;
using System.Text.Json;
using Serilog;
using UnityEngine;

namespace Chaos
{
    public sealed class WebViewMessageListener : MonoBehaviour
    {
        public Action<WebMessageInfo> OnMessageReceived;

        /// <summary>
        /// 接收 Web 端消息
        /// </summary>
        /// <param name="content"></param>
        public void OnWebMessageReceive(string content)
        {
            var info = JsonSerializer.Deserialize<WebMessageInfo>(content, Options.DefaultJsonSerializerOptions);
            OnMessageReceived?.Invoke(info);
        }

        /// <summary>
        /// 接收 Native 端日志
        /// </summary>
        /// <param name="content"></param>
        public void OnNativeLogReceive(string content)
        {
            Log.Information("NativeLog: {content}", content);
        }
    }
}