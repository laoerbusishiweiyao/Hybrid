namespace Chaos
{
    [InvokeHandler(SceneType.Client)]
    public sealed class FiberInitialize_Client : InvokeHandler<FiberInitializeEventArgs, ThreadTask>
    {
        public override async ThreadTask Handle(FiberInitializeEventArgs eventArgs)
        {
            var root = eventArgs.Fiber.Root;

            EntityReference<Scene> reference = root;
            root.AddComponent<MailboxComponent, int>(MailboxType.UnorderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            // root.AddComponent<PlayerComponent>();
            // root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ObjectWait>();
            // root.AddComponent<QuestComponent>();
            // root.AddComponent<ItemComponent>();

            root = reference;
            await EventSystem.Default.PublishAsync(root, new AppStartInitializeFinishEventArgs());
        }
    }
}