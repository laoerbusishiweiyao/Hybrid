using System.Text;
using Chaos;
using Serilog;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;

    Log.Information("Application is shutting down...");
    Log.CloseAndFlush();

    Environment.Exit(0);
};

AvoidCut.Initialize();

Init init = new();
init.Start();

while (true)
{
    Thread.Sleep(1);
    try
    {
        init.Update();
        init.LateUpdate();
    }
    catch (Exception exception)
    {
        Log.Error("{exception}", exception);
    }
}