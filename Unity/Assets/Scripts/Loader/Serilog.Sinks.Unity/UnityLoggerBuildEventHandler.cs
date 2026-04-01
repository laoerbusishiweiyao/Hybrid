using Serilog;

namespace Chaos
{
    [InvokeHandler]
    public sealed class UnityLoggerBuildEventHandler : InvokeHandler<LoggerBuildEventArgs, ILogger>
    {
        public override ILogger Handle(LoggerBuildEventArgs args)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Unity()
                .CreateLogger();
        }
    }
}