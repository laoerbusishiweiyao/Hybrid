namespace Chaos
{
    [EventHandler(SceneType.All)]
    public sealed class EntrySharedPhaseEventHandler : EventHandler<Scene, EntrySharedPhaseEventArgs>
    {
        protected override async ThreadTask RunAsync(Scene scene, EntrySharedPhaseEventArgs eventArgs)
        {
            scene.AddComponent<TimerComponent>();
            scene.AddComponent<CoroutineLockComponent>();
            scene.AddComponent<ObjectWait>();
            scene.AddComponent<MailboxComponent, int>(MailboxType.UnorderedMessage);
            scene.AddComponent<ProcessInnerSender>();

            await ThreadTask.CompletedTask;
        }
    }
}