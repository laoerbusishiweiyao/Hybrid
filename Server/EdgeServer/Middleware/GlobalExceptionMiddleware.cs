using System.Net;
using EdgeServer.Model;

namespace EdgeServer.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentException argumentException => new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = argumentException.Message,
                Payload = null
            },
            KeyNotFoundException => new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "资源查找失败",
                Payload = null
            },
            UnauthorizedAccessException => new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "权限不足",
                Payload = null
            },
            _ => new ApiResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "服务器内部错误",
                Payload = null
            }
        };

        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder) => builder.UseMiddleware<GlobalExceptionMiddleware>();
}