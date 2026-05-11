using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentValidation;
using IdentityService.Core.DTOs;
using IdentityService.Core.Exceptions;

namespace IdentityService.Endpoints.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        if (exception is not ValidationException)
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

        var (statusCode, errorCode, message) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "VALIDATION_ERROR",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", exception.Message),
            ConflictException => (HttpStatusCode.Conflict, "CONFLICT", exception.Message),
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

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
