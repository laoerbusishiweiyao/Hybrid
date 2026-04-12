using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Serilog;
using UnityEngine;

namespace Chaos
{
    [EntitySystemOf(typeof(WebUiComponent))]
    public static partial class WebUiComponentSystem
    {
        [EntitySystem]
        private static void Awake(this WebUiComponent self, string address)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
#if UNITY_EDITOR
            var launchPath = "../Release/Windows/Hybrid.Native.Windows.exe";
#else
            var launchPath = "Hybrid.Native.Windows.exe";
#endif
            var mapName = Guid.NewGuid().ToString("N");
            self.AddComponent<MemoryMappedFileComponent, string, MemoryMappedFileRole>(mapName, MemoryMappedFileRole.Server)
                .Initialize(mapName, address, launchPath, Application.isEditor);
#elif UNITY_ANDROID
#endif

            Application.focusChanged += self.OnFocusChanged;
        }

        [EntitySystem]
        private static void Destroy(this WebUiComponent self)
        {
            Application.focusChanged -= self.OnFocusChanged;
        }

        private static void OnFocusChanged(this WebUiComponent self, bool hasFocus)
        {
#if !UNITY_EDITOR && UNITY_STANDALONE
            self.Send(new Unity2WpfFocusChangedMessage
            {
                HasFocus = hasFocus
            });
#endif
        }

        public static void Send(this WebUiComponent self, IMessage message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            self.GetComponent<MemoryMappedFileComponent>().Send(message);
#elif UNITY_ANDROID
            self.GetComponent<WebViewComponent>().Send(message);
#endif
        }

        public static async ThreadTask<IResponse> SendAsync(this WebUiComponent self, IRequest message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return await self.GetComponent<MemoryMappedFileComponent>().SendAsync(message);
#elif UNITY_ANDROID
            return await self.GetComponent<WebViewComponent>().SendAsync(message);
#endif
        }
    }
}