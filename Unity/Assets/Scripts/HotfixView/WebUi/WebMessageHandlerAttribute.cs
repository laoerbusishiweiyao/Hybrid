namespace Chaos
{
    public sealed class WebMessageHandlerAttribute : BaseAttribute
    {
        public readonly int SceneType;

        public WebMessageHandlerAttribute(int sceneType)
        {
            SceneType = sceneType;
        }
    }
}