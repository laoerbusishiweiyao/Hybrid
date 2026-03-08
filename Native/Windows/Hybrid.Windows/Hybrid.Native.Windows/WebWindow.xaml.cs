using System.Windows;
using CefSharp;
using CefSharp.Event;
using Serilog;

namespace Hybrid.Native.Windows;

public partial class WebWindow : Window
{
    public WebWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // this.Owner = WindowComponent.Default.GetWindow<EmbeddedApplicationWindow>();
        // this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        // this.Width = this.Owner.Width;
        // this.Height = this.Owner.Height;

        LoadBrowser();
    }

    private void LoadBrowser()
    {
        Browser.LifeSpanHandler = new LifeSpanHandler();
        Browser.MenuHandler = new ContextMenuHandler();
        Browser.Address = "https://bing.com/";

        Browser.LoadingStateChanged += OnLoadingStateChanged;
        Browser.JavascriptObjectRepository.ResolveObject += OnResolveObject;
        Browser.JavascriptObjectRepository.ObjectsBoundInJavascript += OnObjectsBoundInJavascript;

        Browser.JavascriptMessageReceived += OnJavascriptMessageReceived;
    }

    private void OnLoadingStateChanged(object? sender, LoadingStateChangedEventArgs eventArgs)
    {
        Application.Current.Dispatcher.Invoke(() => OnLoadingStateChanged(eventArgs));
    }

    private async void OnLoadingStateChanged(LoadingStateChangedEventArgs eventArgs)
    {
        if (!eventArgs.IsLoading)
        {
            Browser.LoadingStateChanged -= OnLoadingStateChanged;

            Log.Information("WebWindow Browser Loaded");

            while (!Browser.CanExecuteJavascriptInMainFrame)
            {
                await Task.Delay(10);
            }

            Log.Information("WebWindow Browser Ready To Execute Javascript");
        }
    }

    private void OnResolveObject(object? sender, JavascriptBindingEventArgs eventArgs)
    {
        // JavascriptBindingComponent.Default.Resolve(eventArgs.ObjectRepository, eventArgs.ObjectName);
    }

    private void OnObjectsBoundInJavascript(object? sender, JavascriptBindingMultipleCompleteEventArgs e)
    {
        foreach (var objectName in e.ObjectNames)
        {
            Log.Debug("绑定成功:{ObjectName}", objectName);
        }
    }

    private void OnJavascriptMessageReceived(object? sender, JavascriptMessageReceivedEventArgs eventArgs)
    {
        // if (JavascriptMessageComponent.Default == null)
        // {
        //     Logger.LogWarning($"JavascriptMessageComponent is null");
        //     return;
        // }
        //
        // try
        // {
        //     var info = eventArgs.ConvertMessageTo<JavascriptMessageInfo>();
        //
        //     Dispatcher.Invoke(() => JavascriptMessageComponent.Default.Handle(Browser, info));
        // }
        // catch (Exception exception)
        // {
        //     Logger.LogError($"javascriptmessage received but can't handle,{exception}");
        // }
    }
}