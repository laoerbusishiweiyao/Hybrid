namespace Chaos
{
    [ComponentOf(typeof(Session))]
    public sealed class SessionAcceptTimeoutComponent : Entity, IAwake, IDestroy
    {
        public long Timer;
    }
}