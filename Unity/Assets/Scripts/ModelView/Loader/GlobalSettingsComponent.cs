namespace Chaos
{
    [ComponentOf(typeof(Scene))]
    public sealed class GlobalSettingsComponent : Entity, IAwake, IDestroy
    {
        public GlobalSettings Settings { get; set; }
    }
}