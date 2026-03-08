using System;
using System.Collections.Generic;
using System.Text.Json;
using TMPro;
using UnityEngine;

namespace Chaos
{
    [DisallowMultipleComponent]
    public sealed class WebMessageDispatcher : MonoBehaviour
    {
        public int MessagePerSecond { get; private set; }
        private readonly Queue<float> messageTimestamps = new();

        // DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() = System.currentTimeMillis()
        public void OnWebMessage(string message)
        {
            messageTimestamps.Enqueue(Time.time);
            var start = Time.time - 1f;
            while (messageTimestamps.Count > 0 && messageTimestamps.Peek() < start)
            {
                messageTimestamps.Dequeue();
            }

            MessagePerSecond = messageTimestamps.Count;

            var info = JsonSerializer.Deserialize<WebMessageInfo>(message, AppSettings.JsonSerializerOptions);

            switch (info.Opcode)
            {
                case Opcode.WebLoaded:
                {
                    GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{info.Opcode} = WebLoaded" + Environment.NewLine;
                    break;
                }
                case Opcode.WebTouchData when info.Payload is JsonElement jsonElement && jsonElement.Deserialize<WebTouchData>(AppSettings.JsonSerializerOptions) is { } webTouchData:
                {
                    GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{info.Opcode} = {info.Payload}" + Environment.NewLine;

                    WebInputAdapter.Process(webTouchData);
                    break;
                }
                case Opcode.AndroidLogEntry when info.Payload is JsonElement jsonElement && jsonElement.Deserialize<AndroidLogEntry>(AppSettings.JsonSerializerOptions) is { } androidLogEntry:
                {
                    GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{info.Opcode} = {info.Payload}" + Environment.NewLine;
                    break;
                }
            }
        }

        private void Update()
        {
            GameObject.Find("/Canvas/WebMessageFps").GetComponent<TextMeshProUGUI>().text =
                $"WebMessage: {MessagePerSecond} / s";
        }
    }
}