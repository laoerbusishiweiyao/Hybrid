using Serilog;

namespace Chaos
{
    [WebMessageHandler(SceneType.WebUi)]
    public sealed class Wpf2UnityLoadedMessageHandler : WebMessageHandler<Wpf2UnityLoadedMessage>
    {
        protected override async ThreadTask RunAsync(WebUiComponent webUiComponent, Wpf2UnityLoadedMessage message)
        {
            Log.Debug("Wpf({id}) Loaded", message.ProcessId);

            webUiComponent.GetComponent<MemoryMappedFileComponent>().ProcessId = message.ProcessId;

            await ThreadTask.CompletedTask;
        }
    }
}