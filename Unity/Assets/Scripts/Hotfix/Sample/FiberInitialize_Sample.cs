namespace Chaos
{
    [InvokeHandler(SceneType.Sample)]
    public sealed class FiberInitialize_Sample : InvokeHandler<FiberInitializeEventArgs, ThreadTask>
    {
        public override async ThreadTask Handle(FiberInitializeEventArgs eventArgs)
        {
            var fiber = eventArgs.Fiber;
            var scene = fiber.Root;

            EntityReference<Scene> reference = scene;
            await EventSystem.Default.PublishAsync(scene, new EntrySharedPhaseEventArgs());
            scene = reference;
            await EventSystem.Default.PublishAsync(scene, new EntryServerPhaseEventArgs());
            scene = reference;
            await EventSystem.Default.PublishAsync(scene, new EntryClientPhaseEventArgs());
        }
    }
}