using System.Text;
using System.Text.Json;
using Chaos;
using Serilog;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;

    Log.Information("Application is shutting down...");
    Log.CloseAndFlush();

    Environment.Exit(0);
};

CSharpBuilder.Build();
TypeScriptBuilder.Build();
WpfMessageBuilder.Build();

Log.Information("All protobuf files were parsed and built");