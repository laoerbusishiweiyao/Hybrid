using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using Serilog;

namespace Hybrid.Native.Windows;

public sealed class Program : Application
{
    #region STAThread

    [STAThread]
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .CreateLogger();

        InitializeCefSettings();

        var program = new Program();
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        Current.Run(new WebWindow());
    }

    #endregion

    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        base.OnStartup(eventArgs);
    }

    protected override void OnExit(ExitEventArgs eventArgs)
    {
        Cef.Shutdown();
        base.OnExit(eventArgs);
    }

    private static void InitializeCefSettings()
    {
        var cachePath = new DirectoryInfo("Cache/CefSharp/").FullName;

        if (Directory.Exists(cachePath))
        {
            Directory.Delete(cachePath, true);
            Log.Information("清理浏览器缓存");
        }

        #region CEF初始化

        var settings = new CefSettings()
        {
            Locale = "zh-CN",
            CachePath = cachePath,
            LogSeverity = LogSeverity.Disable,
            LogFile = new FileInfo($"Logs/HybridNativeWindow.CefSharp.{DateTime.Now}.log").FullName,
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

        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        CefSharpSettings.ConcurrentTaskExecution = true;

        #endregion
    }
}