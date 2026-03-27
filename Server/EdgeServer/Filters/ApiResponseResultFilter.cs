using EdgeServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EdgeServer.Filters;

public sealed class ApiResponseResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        var requestId = context.HttpContext.TraceIdentifier;
        context.HttpContext.Response.Headers["X-Request-Id"] = requestId;

        if (context.Result is ObjectResult { Value: ApiResponse })
        {
            return;
        }

        context.Result = context.Result switch
        {
            OkObjectResult result => Wrap(result.Value, 200, "Success"),
            OkResult _ => Wrap(null, 200, "Success"),
            CreatedResult result => Wrap(result.Value, 201, "Created"),
            BadRequestObjectResult result => Wrap(result.Value, 400, GetErrorMessage(result.Value)),
            NotFoundObjectResult result => Wrap(result.Value, 404, GetErrorMessage(result.Value)),
            NotFoundResult _ => Wrap(null, 404, "Not Found"),
            UnauthorizedResult _ => Wrap(null, 401, "Unauthorized"),
            ForbidResult _ => Wrap(null, 403, "Forbidden"),
            StatusCodeResult status => Wrap(null, status.StatusCode, GetStatusMessage(status.StatusCode)),
            _ => context.Result
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static ObjectResult Wrap(object? payload, int statusCode, string message)
    {
        var response = new ApiResponse
        {
            StatusCode = statusCode,
            Message = message,
            Payload = payload,
        };
        return new ObjectResult(response)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/json" }
        };
    }

    private static string GetErrorMessage(object? value) => value?.ToString() ?? "An error occurred";

    private static string GetStatusMessage(int statusCode) =>
        statusCode switch
        {
            200 => "Success",
            201 => "Created",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => $"Status {statusCode}"
        };
}