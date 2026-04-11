namespace Chaos
{
    public interface IWebMessage : IMessage
    {
    }

    public interface IWebRequest : IWebMessage, IRequest
    {
    }

    public interface IWebResponse : IWebMessage, IResponse
    {
    }
}