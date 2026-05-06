using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TrainService.Services;

namespace TrainService.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, auditService);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IAuditService auditService)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        var statusCode = HttpStatusCode.InternalServerError;
        var errorCode = "INTERNAL_ERROR";
        var message = "An unexpected error occurred. Please try again later.";

        switch (exception)
        {
            case System.Collections.Generic.KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = exception.Message;
                break;
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_INPUT";
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "UNAUTHORIZED";
                message = exception.Message;
                break;
            case InvalidOperationException when exception.Message.Contains("Insufficient seats"):
                statusCode = HttpStatusCode.Conflict;
                errorCode = "INSUFFICIENT_SEATS";
                message = exception.Message;
                break;
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "INVALID_OPERATION";
                message = exception.Message;
                break;
        }

        _logger.LogError(exception, "Error occurred: {Message}", exception.Message);
        auditService.LogError(context.Request.Path, exception.Message, exception);

        var response = new
        {
            errorCode,
            message,
            timestamp = DateTime.UtcNow,
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
