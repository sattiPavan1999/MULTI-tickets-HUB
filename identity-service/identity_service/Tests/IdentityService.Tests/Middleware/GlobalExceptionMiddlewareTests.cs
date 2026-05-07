using System.Net;
using System.Text.Json;
using IdentityService.Endpoints.Middleware;
using IdentityService.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityService.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddlewareTests()
    {
        _logger = new LoggerFactory().CreateLogger<GlobalExceptionMiddleware>();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
            throw new UnauthorizedAccessException("Invalid credentials");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("UNAUTHORIZED", errorResponse.ErrorCode);
        Assert.Equal("Invalid credentials", errorResponse.Message);
    }

    [Fact]
    public async Task InvokeAsync_EmailExistsException_Returns409()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
            throw new InvalidOperationException("Email already registered");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("EMAIL_EXISTS", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_NotFoundExcept_Returns404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
            throw new InvalidOperationException("User not found");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
            throw new Exception("Unexpected error");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("INTERNAL_ERROR", errorResponse.ErrorCode);
        Assert.Equal("An error occurred processing your request", errorResponse.Message);
    }

    [Fact]
    public async Task InvokeAsync_IncludesTraceId_InErrorResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id";

        RequestDelegate next = (HttpContext ctx) =>
            throw new Exception("Test exception");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task InvokeAsync_SetsContentType_ToApplicationJson()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (HttpContext ctx) =>
            throw new Exception("Test exception");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_IncludesTimestamp_InErrorResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var beforeException = DateTime.UtcNow;

        RequestDelegate next = (HttpContext ctx) =>
            throw new Exception("Test exception");

        var middleware = new GlobalExceptionMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        var afterException = DateTime.UtcNow;

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.True(errorResponse.Timestamp >= beforeException);
        Assert.True(errorResponse.Timestamp <= afterException);
    }
}
