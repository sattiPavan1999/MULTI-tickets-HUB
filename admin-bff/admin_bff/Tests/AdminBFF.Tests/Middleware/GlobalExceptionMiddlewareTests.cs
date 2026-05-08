using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using AdminBFF.Endpoints.Middleware;

namespace AdminBFF.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private static async Task<(int StatusCode, string Body)> InvokeMiddleware(Exception ex)
    {
        var context = new DefaultHttpContext();
        var body = new MemoryStream();
        context.Response.Body = body;

        var middleware = new GlobalExceptionMiddleware(
            _ => throw ex,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(body).ReadToEndAsync();
        return (context.Response.StatusCode, responseBody);
    }

    [Fact]
    public async Task UnauthorizedAccessException_Returns401()
    {
        var (status, _) = await InvokeMiddleware(new UnauthorizedAccessException("unauthorized"));
        status.Should().Be(401);
    }

    [Fact]
    public async Task UnhandledException_Returns500()
    {
        var (status, _) = await InvokeMiddleware(new InvalidOperationException("boom"));
        status.Should().Be(500);
    }

    [Fact]
    public async Task Response_ContainsErrorCode()
    {
        var (_, body) = await InvokeMiddleware(new UnauthorizedAccessException("denied"));
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("errorCode").GetString().Should().Be("UNAUTHORIZED");
    }
}
