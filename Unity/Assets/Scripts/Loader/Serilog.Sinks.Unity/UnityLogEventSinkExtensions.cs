using System;
using System.IO;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Chaos
{
    public static class UnityLogEventSinkExtensions
    {
        public const string DefaultUnityDebugOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Scene}] {Message:lj}{NewLine}{Exception}";

        public static LoggerConfiguration Unity(this LoggerSinkConfiguration loggerConfiguration, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, string outputTemplate = DefaultUnityDebugOutputTemplate, IFormatProvider formatProvider = null, LoggingLevelSwitch levelSwitch = null)
        {
            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return loggerConfiguration.Sink(new UnityLogEventSink(formatter), restrictedToMinimumLevel, levelSwitch);
        }

        public static LoggerConfiguration Editor(this LoggerSinkConfiguration loggerConfiguration, string output, LogEventLevel minimumLevel)
        {
            return loggerConfiguration.File(Path.Combine(output, "Unity.log"),
                outputTemplate: DefaultUnityDebugOutputTemplate,
                restrictedToMinimumLevel: minimumLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 28,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(10));
        }

        public static LoggerConfiguration Windows(this LoggerSinkConfiguration loggerConfiguration, string output, LogEventLevel minimumLevel)
        {
            return loggerConfiguration.File(Path.Combine(output, "Unity.log"),
                outputTemplate: DefaultUnityDebugOutputTemplate,
                restrictedToMinimumLevel: minimumLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 28,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(10));
        }

        public static LoggerConfiguration Android(this LoggerSinkConfiguration loggerConfiguration, string output, LogEventLevel minimumLevel)
        {
            return loggerConfiguration.File(Path.Combine(output, "Unity.log"),
                outputTemplate: DefaultUnityDebugOutputTemplate,
                restrictedToMinimumLevel: minimumLevel,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 28,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(10));
        }
    }
}