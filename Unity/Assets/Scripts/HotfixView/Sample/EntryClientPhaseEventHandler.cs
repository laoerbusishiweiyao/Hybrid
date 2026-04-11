namespace Chaos
{
    [EventHandler(SceneType.Sample)]
    public sealed class EntryClientPhaseEventHandler : EventHandler<Scene, EntryClientPhaseEventArgs>
    {
        protected override async ThreadTask RunAsync(Scene root, EntryClientPhaseEventArgs eventArgs)
        {
            var fiber = root.Fiber;

            await fiber.CreateFiberAsync(SchedulerType.Parent, IdGenerator.Default.GenerateId(), 0, SceneType.Client, nameof(SceneType.Client));
            await fiber.CreateFiberAsync(SchedulerType.Parent, IdGenerator.Default.GenerateId(), 0, SceneType.WebUi, nameof(SceneType.WebUi));
        }
    }
}