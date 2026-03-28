using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Chaos
{
    public static class UnityLogEventSinkExtensions
    {
        private const string DefaultUnityDebugOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Fiber}] {Message:lj}{NewLine}{Exception}";

        public static LoggerConfiguration Unity(this LoggerSinkConfiguration loggerConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultUnityDebugOutputTemplate, IFormatProvider formatProvider = null,
            LoggingLevelSwitch levelSwitch = null)
        {
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return loggerConfiguration.Sink(new UnityLogEventSink(formatter), restrictedToMinimumLevel, levelSwitch);
        }
    }
}