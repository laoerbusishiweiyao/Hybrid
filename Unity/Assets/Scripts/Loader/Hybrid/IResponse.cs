namespace Chaos
{
    public interface IResponse : IRequest
    {
        int StatusCode { get; set; }
        string Message { get; set; }
    }
}