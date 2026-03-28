using Serilog;
using Serilog.Core;
using UnityEditor;

namespace Chaos
{
    [InitializeOnLoad]
    public static class UnityLogEventSinkBuilder
    {
        static UnityLogEventSinkBuilder()
        {
            if (Log.Logger == Logger.None)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithProperty("Fiber", "Editor")
                    .MinimumLevel.Verbose()
                    .WriteTo.Unity()
                    .CreateLogger();
            }
        }
    }
}