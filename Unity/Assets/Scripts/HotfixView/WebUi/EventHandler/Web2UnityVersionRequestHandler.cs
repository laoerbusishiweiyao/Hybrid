using UnityEngine;

namespace Chaos
{
    [WebMessageHandler(SceneType.WebUi)]
    public sealed class Web2UnityVersionRequestHandler : WebMessageHandler<Web2UnityVersionRequest, Unity2WebVersionResponse>
    {
        protected override async ThreadTask RunAsync(WebUiComponent webUiComponent, Web2UnityVersionRequest request, Unity2WebVersionResponse response)
        {
            response.Version = Application.version;
            response.UnityVersion = Application.unityVersion;

            await ThreadTask.CompletedTask;
        }
    }
}