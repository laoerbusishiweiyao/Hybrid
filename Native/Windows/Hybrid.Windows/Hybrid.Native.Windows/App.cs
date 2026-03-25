using System.Windows;

namespace Hybrid.Native.Windows;

public sealed class App(WebWindow window) : Application
{
    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        base.OnStartup(eventArgs);

        window.Show();
    }
}