namespace Chaos
{
    public readonly struct NetComponentOnReadEventArgs
    {
        public readonly EntityReference<Session> Session;
        public readonly object Message;

        public NetComponentOnReadEventArgs(Session session, object message)
        {
            Session = session;
            Message = message;
        }
    }

    [ComponentOf(typeof(Scene))]
    public sealed class NetComponent : Entity, IAwake<IKcpTransport>, IDestroy, IUpdate
    {
        public Service Service { get; set; }
    }
}