namespace Chaos;

public sealed partial class Scene
{
    private EntityReference<CoroutineLockComponent> coroutineLockComponent;

    public CoroutineLockComponent CoroutineLockComponent
    {
        get => coroutineLockComponent;
        set => coroutineLockComponent = value;
    }
}

[DisableGetComponent]
[ComponentOf(typeof(Scene))]
public sealed class CoroutineLockComponent : Entity, IAwake, IScene, IUpdate
{
    public Fiber Fiber { get; set; }
    public int SceneType { get; set; }

    public readonly Queue<(long, long, int)> DeferredContinuations = new();
}