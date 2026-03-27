using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Chaos
{
    public sealed record WebLoaded : MessageObject, IWebMessage
    {
    }

    public sealed class WebLoadedHandler : WebMessageHandler<WebLoaded>
    {
        protected override Task RunAsync(WebLoaded message)
        {
            GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text = $"received WebLoaded({DateTime.Now.ToLongTimeString()}): {message}\n";
            return Task.CompletedTask;
        }
    }
}