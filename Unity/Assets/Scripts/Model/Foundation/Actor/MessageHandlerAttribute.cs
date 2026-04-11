namespace Chaos
{
    public sealed class MessageHandlerAttribute : BaseAttribute
    {
        public readonly int SceneType;

        public MessageHandlerAttribute(int sceneType)
        {
            SceneType = sceneType;
        }
    }
}