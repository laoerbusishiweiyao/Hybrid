using System;
using System.IO;
using Serilog;
using Serilog.Events;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Chaos
{
    public sealed class Program : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            if (Log.Logger != Serilog.Core.Logger.None)
            {
                Log.CloseAndFlush();
            }

            Log.Logger = new LoggerConfiguration()
#if UNITY_EDITOR
                .WriteTo.Unity()
#elif UNITY_STANDALONE
                .WriteTo.Unity()
                .WriteTo.Windows("Logs", LogEventLevel.Debug)
#elif UNITY_ANDROID
                .WriteTo.Android(Path.Combine(Application.persistentDataPath, "Logs"), LogEventLevel.Debug)
#endif
                .CreateLogger().ForContext("Scene", "Main");
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnAfterAssembliesLoaded()
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashScreen()
        {
            var gameObject = new GameObject(nameof(UnityEngine), typeof(Program));
            DontDestroyOnLoad(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
        }

        private void Awake()
        {
            InitializeAsync().Coroutine();
        }

        private async ThreadTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => Log.Error("{exception}", eventArgs.ExceptionObject);

            World.Default.AddSingleton(new Options
            {
                SceneName = Resources.Load<GlobalSettings>(nameof(GlobalSettings)).SceneName
            });

#if (HIERARCHY && UNITY_EDITOR) || UNITY_WEBGL
            Options.Default.SingleThread = 1;
#endif

            ThreadTask.ExceptionHandler += exception => Log.Error("{exception}", exception);

            World.Default.AddSingleton<TimeInfo>();
            World.Default.AddSingleton<FiberRegistry>();

            await World.Default.AddSingleton<YooAssetsComponent>().CreatePackageAsync("DefaultPackage", true);

            World.Default.AddSingleton<CodeLoader>().StartAsync().Coroutine();
        }

        private void OnEnable()
        {
            TouchSimulation.Enable();
        }

        private void Update()
        {
            FiberRegistry.Default.Update();
        }

        private void LateUpdate()
        {
            FiberRegistry.Default.LateUpdate();
        }

        private void OnDisable()
        {
            TouchSimulation.Disable();
        }

        private void OnApplicationQuit()
        {
            World.Default.Dispose();
            Log.Information("程序退出");
            Log.CloseAndFlush();
        }
    }
}