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
            if (ex is not ValidationException)
                logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, errorCode, message) = ex switch
        {
            ValidationException ve => (400, "VALIDATION_ERROR", string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
            NotFoundException => (404, "NOT_FOUND", ex.Message),
            ConflictException => (409, "CONFLICT", ex.Message),
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
