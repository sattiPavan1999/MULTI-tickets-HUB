using System.Diagnostics;
using System.Net;
using System.Text.Json;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;

namespace IdentityService.Endpoints.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        var (statusCode, errorCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", exception.Message),
            ConflictException => (HttpStatusCode.Conflict, "EMAIL_EXISTS", exception.Message),
            NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An error occurred processing your request")
        };

        var errorResponse = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(errorResponse);

        return context.Response.WriteAsync(json);
    }
}
