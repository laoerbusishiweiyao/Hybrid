using System;
using System.Threading;
using Serilog;
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

            // var configuration = new ConfigurationBuilder()
            //     .SetBasePath(AppContext.BaseDirectory)
            //     .AddJsonFile("appsettings.json", false)
            //     .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Unity()
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


        // private void Awake()
        // {
        //     AppDomain.CurrentDomain.UnhandledException += (sender, args) => Debug.LogError(args.ExceptionObject);
        //
        //     unitySynchronizationContext = SynchronizationContext.Current;
        //     SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Default);
        //
        //     _ = WebContext.InitializeAsync("http://192.168.10.31:12345/");
        // }
        //
        // private void Update()
        // {
        //     ThreadSynchronizationContext.Default.Update();
        // }
        //
        // private void OnApplicationQuit()
        // {
        //     WebMessageDispatcher.Default.Dispose();
        //     NamedPipeSession.Default.Dispose();
        //     WebContext.Shutdown();
        //
        //     SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);
        //     unitySynchronizationContext = null;
        // }
    }
}