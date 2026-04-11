using System;
using Serilog;

namespace Chaos
{
    [EventHandler(SceneType.WebUi)]
    public sealed class WebUiInitializeFinishEventHandler : EventHandler<Scene, WebUiInitializeFinishEventArgs>
    {
        protected override async ThreadTask RunAsync(Scene scene, WebUiInitializeFinishEventArgs eventArgs)
        {
            scene.AddComponent<GlobalSettingsComponent>();

            scene.AddComponent<WebUiComponent, string>("http://127.0.0.1:12345");

// #if UNITY_EDITOR || UNITY_STANDALONE
//             scene.AddComponent<NamedPipeClientComponent, string>(webUiComponent.NamedPipeName);
//             scene.AddComponent<NamedPipeSessionComponent>();
// #endif
//
            // scene.GetComponent<WebUiComponent>().Send(new Unity2WebLoadedMessage());
            // try
            // {
            //     var currentFiberId = scene.Fiber.Id;
            //     Log.Warning("📤 [SEND] About to Await: FiberId={FiberId}", currentFiberId);
            //     var response = await scene.GetComponent<WebUiComponent>().SendAsync(new Unity2WebUserAgentRequest()) as Web2UnityUserAgentResponse;
            //     Log.Warning("✅ [SEND] Continuation Executed!");
            //     Log.Information("WebUi UserAgent: {ua}", response?.UserAgent);
            // }
            // catch (Exception exception)
            // {
            //     var currentFiberId = scene.Fiber.Id;
            //     Log.Warning("exception({d}): {message}", currentFiberId, exception);
            // }

            await ThreadTask.CompletedTask;
        }
    }
}