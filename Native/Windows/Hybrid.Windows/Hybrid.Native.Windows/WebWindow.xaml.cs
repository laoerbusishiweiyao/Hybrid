using System.Windows;
using CefSharp;
using Microsoft.Extensions.Options;
using Serilog;

namespace Hybrid.Native.Windows;

public partial class WebWindow : Window
{
    private readonly IOptions<AppSettings> settings;

    private readonly NamedPipeSession session;

    public WebWindow(IOptions<AppSettings> settings, NamedPipeSession session)
    {
        this.settings = settings;
        this.session = session;

        Log.Information("启动参数: {settings}", settings.Value);

        InitializeComponent();
        Loaded += OnLoaded;

        session.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object? sender, NamedPipeMessageEventArgs eventArgs)
    {
        Log.Debug("receive {opcode} = {content}", eventArgs.Opcode, eventArgs.Payload);
        Browser.EvaluateScriptAsync($"window.receive({eventArgs.Opcode}, {eventArgs.Payload})");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Left = settings.Value.LaunchOptions.Left;
        Top = settings.Value.LaunchOptions.Top;
        Width = settings.Value.LaunchOptions.Width;
        Height = settings.Value.LaunchOptions.Height;

        LoadBrowser();
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

            Topmost = true;

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

        session.Send(message);
    }
}