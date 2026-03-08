using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine.Android;
#endif

namespace Chaos
{
    public static class WebBridge
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string BridgeClassFullName = "com.chaosstudio.nativebridge.WebBridge";

        private static AndroidJavaObject currentActivity;

        /// <summary>
        /// 获取当前 Android Activity (即 Context)
        /// </summary>
        private static AndroidJavaObject CurrentActivity
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
#endif

        public static void Load(AndroidWebViewOptions options)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var bridge = new AndroidJavaClass(BridgeClassFullName))
                {
                    bridge.CallStatic("load", CurrentActivity, JsonSerializer.Serialize(options, AppSettings.JsonSerializerOptions));
                }

                Debug.Log("[Unity] Load WebView");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Unity] Load WebView Failed: {exception.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"[Editor] Load WebView\n{JsonSerializer.Serialize(options, AppSettings.JsonSerializerOptions)}");
#endif
        }

        public static void Unload()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var bridge = new AndroidJavaClass(BridgeClassFullName))
                {
                    bridge.CallStatic("unload", CurrentActivity);
                }

                Debug.Log("[Unity] Unload WebView");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Unity] Unload WebView Failed: {exception.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log("[Editor] Unload WebView");
#endif
        }

        public static void Send(string message)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var bridge = new AndroidJavaClass(BridgeClassFullName))
                {
                    bridge.CallStatic("send", CurrentActivity, message);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Unity] Send Failed: {exception.Message}");
            }
#elif UNITY_EDITOR
            Debug.Log($"[Editor] Send: {message}");
#endif
        }

        public static async Task CopyStreamingAssetsToPersistentAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var request = Resources.LoadAsync<TextAsset>("WebUIManifest");
            request.completed += _ => taskCompletionSource.SetResult(true);
            await taskCompletionSource.Task;
            if (request.asset is not TextAsset asset)
            {
                return;
            }

            foreach (var source in JsonSerializer.Deserialize<string[]>(asset.text))
            {
                await CopyAsync(Path.Combine(Application.streamingAssetsPath, source),
                    Path.Combine(Application.persistentDataPath, source));
            }
        }

        private static async Task CopyAsync(string source, string destination)
        {
            if (Application.platform == RuntimePlatform.Android && !Application.isEditor)
            {
                using var request = UnityWebRequest.Get(source);
                await request.SendWebRequest();
                var buffer = request.downloadHandler.data;

                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(destination, buffer);
            }

            if (Application.isEditor)
            {
                var buffer = await File.ReadAllBytesAsync(source);

                var directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(destination, buffer);
            }
        }
    }
}