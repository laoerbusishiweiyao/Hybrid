namespace Chaos
{
    public interface IRequest : IMessage
    {
        int RequestId { get; set; }
    }
}