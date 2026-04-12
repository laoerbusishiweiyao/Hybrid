using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Windows.Win32;
using CefSharp;
using Microsoft.Extensions.Options;
using Serilog;

namespace Hybrid.Native.Windows;

public partial class WebWindow : Window
{
    private readonly IOptions<AppSettings> settings;

    private readonly MemoryMappedFileSession session;

    private DispatcherTimer? dispatcherTimer;

    public WebWindow(IOptions<AppSettings> settings, MemoryMappedFileSession session)
    {
        this.settings = settings;
        this.session = session;

        Log.Debug("启动参数: {settings}", settings.Value);

        InitializeComponent();

        AddUnityFocusEventListener();

        Loaded += OnLoaded;

        session.MessageReceived += OnMessageReceived;

        Send(Opcode.Wpf2UnityLoadedMessage, new Wpf2UnityLoadedMessage { ProcessId = Environment.ProcessId });
    }

    private void AddUnityFocusEventListener()
    {
        if (settings.Value.LaunchOptions.ProcessId is 0)
        {
            return;
        }

        dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        dispatcherTimer.Tick += OnDispatcherTimerTick;
        dispatcherTimer.Start();
    }

    private void OnDispatcherTimerTick(object? sender, EventArgs eventArgs)
    {
        var foregroundWindow = PInvoke.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
        {
            return;
        }

        PInvoke.GetWindowThreadProcessId(foregroundWindow, out var processId);

        if (Process.GetProcessById((int)processId) is not { } process)
        {
            return;
        }

        OnUnityFocusChanged(process.ProcessName is "Hybrid.Native.Windows" or "Unity");
    }

    private void OnMessageReceived(object? sender, MemoryMappedFileReceivedEventArgs eventArgs)
    {
        Log.Debug("receive {opcode} = {content}", eventArgs.Opcode, eventArgs.Payload);

        switch (eventArgs.Opcode)
        {
            case Opcode.Unity2WpfFocusChangedMessage:
            {
                if (JsonSerializer.Deserialize<Unity2WpfFocusChangedMessage>(eventArgs.Payload, AppSettings.DefaultJsonSerializerOptions) is not { } payload)
                {
                    return;
                }

                if (CheckAccess())
                {
                    OnUnityFocusChanged(payload.HasFocus);
                }
                else
                {
                    Dispatcher.Invoke(() => OnUnityFocusChanged(payload.HasFocus));
                }

                return;
            }
            case Opcode.Unity2WpfShutdownMessage:
            {
                if (JsonSerializer.Deserialize<Unity2WpfShutdownMessage>(eventArgs.Payload, AppSettings.DefaultJsonSerializerOptions) is not { } payload)
                {
                    return;
                }

                if (CheckAccess())
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    Dispatcher.Invoke(() => Application.Current.Shutdown());
                }

                return;
            }
            default:
                Browser.EvaluateScriptAsync($"window.receive({eventArgs.Opcode}, {eventArgs.Payload})");
                break;
        }
    }

    private void OnUnityFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            Topmost = true;
            Visibility = Visibility.Visible;
        }
        else
        {
            Topmost = false;
            Visibility = Visibility.Hidden;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (settings.Value.LaunchOptions.ProcessId is 0)
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
        }
        else
        {
            Left = settings.Value.LaunchOptions.Left;
            Top = settings.Value.LaunchOptions.Top;
            Width = settings.Value.LaunchOptions.Width;
            Height = settings.Value.LaunchOptions.Height;

            if (Process.GetProcessById(settings.Value.LaunchOptions.ProcessId) is { HasExited: false } process)
            {
                process.EnableRaisingEvents = true;
                process.Exited += OnProcessExited;
            }
        }

        LoadBrowser();
    }

    private void OnProcessExited(object? sender, EventArgs eventArgs)
    {
        Log.Information("Unity Process {id} exited", settings.Value.LaunchOptions.ProcessId);
        if (CheckAccess())
        {
            Application.Current.Shutdown();
        }
        else
        {
            Dispatcher.Invoke(() => Application.Current.Shutdown());
        }
    }

    private void LoadBrowser()
    {
        Browser.LifeSpanHandler = new LifeSpanHandler();
        Browser.MenuHandler = new ContextMenuHandler();
        Browser.Address = settings.Value.LaunchOptions.Address;

        Browser.FrameLoadEnd += OnFrameLoadEnd;
        Browser.LoadingStateChanged += OnLoadingStateChanged;

        Browser.JavascriptMessageReceived += OnJavascriptMessageReceived;
    }

    private void OnFrameLoadEnd(object? sender, FrameLoadEndEventArgs eventArgs)
    {
        if (!eventArgs.Frame.IsMain)
        {
            return;
        }

        Browser.ExecuteScriptAsync("window.dispatchEvent(new CustomEvent('platformReady', { detail: { platform: 'windows' } }));");
    }

    private void OnLoadingStateChanged(object? sender, LoadingStateChangedEventArgs eventArgs)
    {
        Application.Current.Dispatcher.InvokeAsync(() => OnLoadingStateChanged(eventArgs));
    }

    private async Task OnLoadingStateChanged(LoadingStateChangedEventArgs eventArgs)
    {
        if (!eventArgs.IsLoading)
        {
            Browser.LoadingStateChanged -= OnLoadingStateChanged;

            while (!Browser.CanExecuteJavascriptInMainFrame)
            {
                await Task.Delay(10);
            }

            if (settings.Value.LaunchOptions.ShowDevTools)
            {
                Browser.ShowDevTools();
            }
        }
    }

    private void OnJavascriptMessageReceived(object? sender, JavascriptMessageReceivedEventArgs eventArgs)
    {
        if (eventArgs.Message is not string message)
        {
            Log.Warning("CefSharp Web Message Type {type} UnSupport.", eventArgs.Message.GetType());
            return;
        }

        if (JsonSerializer.Deserialize<MessageInfo>(message, AppSettings.DefaultJsonSerializerOptions) is not { } info)
        {
            Log.Warning("CefSharp Web Message Deserialize Failed.\n{message}", message);
            return;
        }

        session.Send(info.Opcode, info.Payload);
    }

    private void Send(ushort opcode, object message)
    {
        session.Send(opcode, message);
    }
}