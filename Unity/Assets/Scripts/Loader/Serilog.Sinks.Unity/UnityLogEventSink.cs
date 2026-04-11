using System.Collections.Generic;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using TMPro;
using UnityEngine;

namespace Chaos
{
    public sealed class UnityLogEventSink : ILogEventSink
    {
        private static readonly Dictionary<LogEventLevel, string> colors = new()
        {
            { LogEventLevel.Verbose, "#888888" },
            { LogEventLevel.Debug, "#66B2FF" },
            { LogEventLevel.Information, "#00CC00" },
            { LogEventLevel.Warning, "#FF9900" },
            { LogEventLevel.Error, "#FF3333" },
            { LogEventLevel.Fatal, "#AA00AA" }
        };

        private readonly ITextFormatter formatter;

        public UnityLogEventSink(ITextFormatter formatter)
        {
            this.formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            using var writer = new StringWriter();
            formatter.Format(logEvent, writer);
            var message = writer.ToString().Trim()
                .Insert(31, $"<color={colors[logEvent.Level]}>")
                .Insert(51, "</color>");
            Debug.Log(message);

            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }

            var console = GameObject.Find("/Canvas/Scroll View/Viewport/WebMessageConsole");
            if (console)
            {
                console.GetComponent<TextMeshProUGUI>().text += $"{message}\n";
            }
        }
    }
}