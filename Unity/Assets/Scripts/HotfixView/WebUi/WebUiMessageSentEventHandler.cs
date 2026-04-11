namespace Chaos
{
    [EventHandler(SceneType.WebUi)]
    public sealed class WebUiMessageSentEventHandler : EventHandler<Scene, WebUiMessageSentEventArgs>
    {
        protected override async ThreadTask RunAsync(Scene scene, WebUiMessageSentEventArgs eventArgs)
        {
            scene.GetComponent<WebUiComponent>().Send(eventArgs.Message);
            
            await ThreadTask.CompletedTask;
        }
    }
}