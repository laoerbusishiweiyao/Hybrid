namespace Chaos
{
    [ComponentOf(typeof(Session))]
    public sealed class SessionIdleCheckerComponent : Entity, IAwake, IDestroy
    {
        public long RepeatedTimer;
    }
}