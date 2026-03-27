namespace EdgeServer.Model;

public record ApiResponse<T> where T : class
{
    public required int StatusCode { get; init; }
    public required string Message { get; init; }
    public required T? Payload { get; init; }
}

public sealed record ApiResponse : ApiResponse<object>
{
}