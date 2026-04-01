namespace Chaos;

public sealed class EventHandlerAttribute : BaseAttribute
{
    public readonly int SceneType;

    public EventHandlerAttribute(int sceneType)
    {
        SceneType = sceneType;
    }
}