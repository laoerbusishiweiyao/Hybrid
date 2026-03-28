namespace Chaos;

public interface IMessage
{
}

public interface IRequest : IMessage
{
    int RequestId { get; set; }
}

public interface IResponse : IMessage
{
    int RequestId { get; set; }
    int StatusCode { get; set; }
    string Message { get; set; }
}