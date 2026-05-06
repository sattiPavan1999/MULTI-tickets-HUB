using System.Diagnostics;
using System.Net;
using System.Text.Json;
using AdminBFF.Models;

namespace AdminBFF.Middleware;

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
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}", traceId);

        var statusCode = exception switch
        {
            AdminBFFException adminEx => adminEx.StatusCode,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var errorCode = exception switch
        {
            AdminBFFException adminEx => adminEx.ErrorCode,
            _ => "INTERNAL_ERROR"
        };

        var message = exception switch
        {
            AdminBFFException => exception.Message,
            _ => "An internal server error occurred"
        };

        var errorResponse = new
        {
            errorCode,
            message,
            timestamp = DateTime.UtcNow,
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
