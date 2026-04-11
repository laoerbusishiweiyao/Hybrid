using Serilog;

namespace Chaos
{
    [WebMessageHandler(SceneType.WebUi)]
    public sealed class Web2UnityLoadedMessageHandler : WebMessageHandler<Web2UnityLoadedMessage>
    {
        protected override async ThreadTask RunAsync(WebUiComponent webUiComponent, Web2UnityLoadedMessage message)
        {
            Log.Information("Web Loaded");

            webUiComponent.Send(new Unity2WebLoadedMessage());

            await ThreadTask.CompletedTask;
        }
    }
}