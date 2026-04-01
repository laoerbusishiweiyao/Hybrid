using Serilog;

namespace Chaos;

public sealed class Init
{
    public void Start()
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) => { Log.Error("{exception}", eventArgs.ExceptionObject); };

            // 命令行参数
            // Parser.Default.ParseArguments<Options>(Environment.GetCommandLineArgs())
            //     .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
            //     .WithParsed((o) => World.Default.AddSingleton(o));
            World.Default.AddSingleton(new Options
            {
                SceneName = "Sample",
            });

            // 测试用例使用单线程模式，方便重置测试环境
            if (Options.Default.SceneName == "Test")
            {
                Options.Default.SingleThread = 1;
                Options.Default.Console = 1;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Scene}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger().ForContext("Scene", Options.Default.SceneName);
            // World.Default.AddSingleton<Logger>().Log = new NLogger(Options.Instance.SceneName);

            ThreadTask.ExceptionHandler += exception => Log.Error("{exception}", exception);

            Log.Information("Application is starting...");

            World.Default.AddSingleton<TimeInfo>();
            World.Default.AddSingleton<FiberRegistry>();
            World.Default.AddSingleton<CodeLoader>().Start();
        }
        catch (Exception exception)
        {
            Log.Error("{exception}", exception);
        }
    }

    public void Update()
    {
        FiberRegistry.Default.Update();
    }

    public void LateUpdate()
    {
        FiberRegistry.Default.LateUpdate();
    }
}