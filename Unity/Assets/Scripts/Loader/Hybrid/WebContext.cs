using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Chaos
{
    public static class WebContext
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        private static Process cefSharp;

        /// <summary>
        /// 初始化网页
        /// </summary>
        /// <param name="address">网页入口地址(默认值为本地加载模式)</param>
        public static async Task InitializeAsync(string address = "https://bing.com/")
        {
            var namedPipeName = Guid.NewGuid().ToString("N");
            cefSharp = Process.Start(new ProcessStartInfo
            {
#if UNITY_EDITOR
                FileName = "../Release/Windows/Hybrid.Native.Windows.exe",
#else
                FileName = "./Hybrid.Native.Windows.exe",
#endif
                ArgumentList =
                {
                    $"--LaunchOptions:ProcessId={Process.GetCurrentProcess().Id}",
                    $"--LaunchOptions:Left={ScreenInformation.Left}",
                    $"--LaunchOptions:Top={ScreenInformation.Top}",
                    $"--LaunchOptions:Width={ScreenInformation.Width}",
                    $"--LaunchOptions:Height={ScreenInformation.Height}",
                    $"--LaunchOptions:NamedPipeName={namedPipeName}",
                    $"--LaunchOptions:Address={address}",
                    $"--LaunchOptions:ShowDevTools={Application.isEditor}"
                },
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            await NamedPipeSession.Default.ConnectAsync(namedPipeName);

            Debug.Log($"NamedPipe Client {namedPipeName} Connected");
        }

        public static void Send(object message)
        {
            var opcode = OpcodeRegistry.Default.FindOpcode(message.GetType());
            NamedPipeSession.Default.Send(opcode, message);
        }

        public static void Shutdown()
        {
            if (cefSharp is not { HasExited: false })
            {
                return;
            }

            cefSharp.CloseMainWindow();

            if (!cefSharp.WaitForExit(5000))
            {
                cefSharp.Kill();
                cefSharp.WaitForExit(1000);
            }

            cefSharp = null;
        }
#elif UNITY_ANDROID
        private const string BridgeClassFullName = "com.chaosstudio.nativebridge.WebViewComponent";

        private static AndroidJavaObject currentActivity;

        /// <summary>
        /// 获取当前 Android Activity (即 Context)
        /// </summary>
        public static AndroidJavaObject CurrentActivity
        {
            get
            {
                if (currentActivity != null)
                {
                    return currentActivity;
                }

                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                return currentActivity;
            }
        }

        /// <summary>
        /// 初始化网页
        /// </summary>
        /// <param name="address">网页入口地址(默认值为本地加载模式)</param>
        /// <param name="domain">本地资源虚拟域名 (拦截域名) - 当请求 URL 的 host 匹配此域名时，请求将被拦截并从本地文件系统加载</param>
        /// <param name="contentPath">本地资源持久化文件夹名称(对应 Android: context.getExternalFilesDir({contentPath}))</param>
        /// <returns></returns>
        public static Task InitializeAsync(string address = "https://chaos.com", string domain = "chaos.com", string contentPath = "WebUI")
        {
            using var component = new AndroidJavaClass(BridgeClassFullName);
            component.CallStatic("load", CurrentActivity, address, domain, contentPath);
            return Task.CompletedTask;
        }

        public static void Send(object message)
        {
            // Android opcode 参数类型为 ushort 报错 NoSuchMethodError
            int opcode = OpcodeRegistry.Default.FindOpcode(message.GetType());
            using var component = new AndroidJavaClass(BridgeClassFullName);
            component.CallStatic("send", CurrentActivity, opcode, System.Text.Json.JsonSerializer.Serialize(message, AppSettings.DefaultJsonSerializerOptions));
        }

        public static void Shutdown()
        {
            using var component = new AndroidJavaClass(BridgeClassFullName);
            component.CallStatic("unload", CurrentActivity);
        }
#endif
    }
}