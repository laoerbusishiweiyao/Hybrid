using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;

namespace Chaos
{
    public sealed class Program : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var gameObject = new GameObject(nameof(Program), typeof(Program), typeof(WebMessageDispatcher));
            DontDestroyOnLoad(gameObject);
        }

        private void Awake()
        {
#if UNITY_EDITOR
            var files = Directory.EnumerateFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories)
                .Where(file => Path.GetExtension(file) != ".meta")
                .Select(file => Path.GetRelativePath(Application.streamingAssetsPath, file).Replace('\\', '/'))
                .ToList();
            File.WriteAllText("Assets/Resources/StreamingAssetsManifest.json", JsonSerializer.Serialize(files));
#else
            _ = InitializeAsync();
#endif
        }

        private async Task InitializeAsync()
        {
            await WebBridge.CopyStreamingAssetsToPersistentAsync();
            WebBridge.Load(new AndroidWebViewOptions());
        }

        private void OnApplicationQuit()
        {
            WebBridge.Unload();
        }
    }
}