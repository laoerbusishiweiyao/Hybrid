using System;
using System.Threading;
using UnityEngine;

namespace Chaos
{
    public sealed class Program : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var gameObject = new GameObject(nameof(Program), typeof(Program));
            DontDestroyOnLoad(gameObject);
        }

        private SynchronizationContext unitySynchronizationContext;

        private void Awake()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Debug.LogError(args.ExceptionObject);

            unitySynchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Default);

            _ = WebContext.InitializeAsync("http://192.168.10.31:12345/");
        }

        private void Update()
        {
            ThreadSynchronizationContext.Default.Update();
        }

        private void OnApplicationQuit()
        {
            WebMessageDispatcher.Default.Dispose();
            NamedPipeSession.Default.Dispose();
            WebContext.Shutdown();

            SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);
            unitySynchronizationContext = null;
        }
    }
}