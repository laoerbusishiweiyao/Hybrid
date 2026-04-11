namespace Chaos
{
    [EventHandler(SceneType.Client)]
    public sealed class AppStartInitializeFinishEventHandler : EventHandler<Scene, AppStartInitializeFinishEventArgs>
    {
        protected override async ThreadTask RunAsync(Scene scene, AppStartInitializeFinishEventArgs eventArgs)
        {
            scene.AddComponent<GlobalSettingsComponent>();

            await ThreadTask.CompletedTask;
        }
    }
}