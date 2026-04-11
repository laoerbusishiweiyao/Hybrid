using System;

namespace Chaos
{
    [InvokeHandler(SceneType.WebUi)]
    public sealed class FiberInitialize_WebUI : InvokeHandler<FiberInitializeEventArgs, ThreadTask>
    {
        public override async ThreadTask Handle(FiberInitializeEventArgs eventArgs)
        {
            var root = eventArgs.Fiber.Root;

            root.AddComponent<MailboxComponent, int>(MailboxType.UnorderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<FiberParentComponent>();
            root.AddComponent<ObjectWait>();

            root.GetComponent<FiberParentComponent>().ParentFiberId = eventArgs.Fiber.Id;

            await EventSystem.Default.PublishAsync(root, new WebUiInitializeFinishEventArgs());
        }
    }
}