namespace Chaos
{
    [ComponentOf(typeof(Scene))]
    public sealed class WebUiComponent : Entity, IAwake<string>, IDestroy
    {
    }
}