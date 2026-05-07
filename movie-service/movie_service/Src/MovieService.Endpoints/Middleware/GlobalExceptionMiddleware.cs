using System.Net;
using System.Text.Json;

namespace MovieService.Endpoints.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        _logger.LogError(exception, "An error occurred. TraceId: {TraceId}", traceId);

        var statusCode = exception switch
        {
            System.Collections.Generic.KeyNotFoundException => HttpStatusCode.NotFound,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.Conflict,
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.InternalServerError
        };

        var errorCode = statusCode switch
        {
            HttpStatusCode.NotFound => "RESOURCE_NOT_FOUND",
            HttpStatusCode.BadRequest => "INVALID_REQUEST",
            HttpStatusCode.Conflict => "CONFLICT",
            HttpStatusCode.Forbidden => "FORBIDDEN",
            _ => "INTERNAL_ERROR"
        };

        var response = new
        {
            errorCode,
            message = statusCode == HttpStatusCode.InternalServerError
                ? "An error occurred while processing your request"
                : exception.Message,
            timestamp = DateTime.UtcNow,
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
