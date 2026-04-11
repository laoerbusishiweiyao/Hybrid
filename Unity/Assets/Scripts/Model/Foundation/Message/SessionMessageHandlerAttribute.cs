namespace Chaos
{
    public sealed class SessionMessageHandlerAttribute : BaseAttribute
    {
        public readonly int SceneType;

        public SessionMessageHandlerAttribute(int sceneType)
        {
            SceneType = sceneType;
        }
    }
}