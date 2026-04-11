using System.IO;
using System.Text;
using Windows.Win32;
using CefSharp;
using CefSharp.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hybrid.Native.Windows;

public sealed class Program
{
    #region STAThread

    [STAThread]
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        _ = PInvoke.timeBeginPeriod(1);

        InitializeCefSettings();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            if (File.Exists(LaunchOptions.DefaultFilePath))
            {
                builder.Configuration.AddJsonFile(LaunchOptions.DefaultFilePath);
                File.Delete(LaunchOptions.DefaultFilePath);
            }

            builder.Services.AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration));
            builder.Services.Configure<AppSettings>(builder.Configuration);
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<WebWindow>();
            
            builder.Services.AddSingleton<MemoryMappedFileSession>();
            builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<MemoryMappedFileSession>());

            var host = builder.Build();

            var app = host.Services.GetRequiredService<App>();
            app.Startup += (_, _) =>
            {
                Log.Information("进程启动({id})", Environment.ProcessId);
                host.StartAsync().GetAwaiter().GetResult();
            };
            app.Exit += (_, _) =>
            {
                host.StopAsync().GetAwaiter().GetResult();

                Log.Information("进程退出({id})", Environment.ProcessId);
                Cef.Shutdown();
                Log.CloseAndFlush();
            };

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                app.Dispatcher.InvokeAsync(() => app.Shutdown());
            };

            app.Run();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "应用启动失败");
        }
        finally
        {
            Cef.Shutdown();
            Log.CloseAndFlush();
        }
    }

    #endregion

    private static void InitializeCefSettings()
    {
        var cachePath = Path.GetFullPath("Cache/CefSharp/");

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
        }

        #region CEF初始化

        var settings = new CefSettings
        {
            Locale = "zh-CN",
            CachePath = cachePath,
            LogSeverity = LogSeverity.Error,
            LogFile = new FileInfo($"../Logs/HybridNativeWindow.CefSharp.{DateTime.Now}.log").FullName,
            // UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36",
        };

        //cefSettings.CefCommandLineArgs.Remove("mute-audio");

        settings.CefCommandLineArgs.Add("proxy-auto-detect", "0");
        settings.CefCommandLineArgs.Add("no-proxy-serve", "1");

        settings.CefCommandLineArgs["disable-gpu"] = "1";

        //启用单进程模式
        //settings.CefCommandLineArgs["single-process"] = "1";

        //settings.MultiThreadedMessageLoop = false;

        //启用媒体流
        settings.CefCommandLineArgs["enable-media-stream"] = "1";

        settings.CefCommandLineArgs["enable-smooth-scrolling"] = "1";

        settings.CefCommandLineArgs["enable-speech-input"] = "1";
        settings.CefCommandLineArgs["enable-speech-synthesis"] = "1";
        settings.CefCommandLineArgs["enable-speech-dispatcher"] = "1";

        //启用音视频自动播放
        settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";

        //启用插件支持
        settings.CefCommandLineArgs.Add("plugin-policy", "allow");

        ////启用Flash
        //cefSettings.CefCommandLineArgs.Add("ppapi-out-of-process");
        //cefSettings.CefCommandLineArgs["ppapi-flash-version"] = "32.0.0.363";
        //cefSettings.CefCommandLineArgs["ppapi-flash-path"] = AppDomain.CurrentDomain.BaseDirectory + "pepflashplayer.dll";

        ////启用插件支持
        //cefSettings.CefCommandLineArgs.Add("plugin-policy", "allow");
        ////启用长按触发的文本选择的拖动操作
        //cefSettings.CefCommandLineArgs.Add("enable-longpress-drag-selection", "1");
        ////启用平滑滚动动画（需要平台支持）
        //cefSettings.CefCommandLineArgs.Add("enable-smooth-scrolling", "1");
        ////禁用代理
        //cefSettings.CefCommandLineArgs.Add("no-proxy-server", "1");

        CefSharpSettings.ShutdownOnExit = true;
        CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
        CefSharpSettings.ConcurrentTaskExecution = true;

        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

        #endregion
    }
}