using System.Text.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;

namespace AdminBFF.Endpoints.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in admin-bff");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, errorCode, message) = ex switch
        {
            ProxyException pex => (pex.StatusCode, $"HTTP_{pex.StatusCode}", pex.Message),
            UnauthorizedAccessException => (401, "UNAUTHORIZED", ex.Message),
            _ => (500, "INTERNAL_ERROR", "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
