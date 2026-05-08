using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieService.Core.Exceptions;
using MovieService.Endpoints.Middleware;

namespace MovieService.Tests.Middleware;

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
    public async Task NotFoundException_Returns404()
    {
        var (status, _) = await InvokeMiddleware(new NotFoundException("not found"));
        status.Should().Be(404);
    }

    [Fact]
    public async Task ConflictException_Returns409()
    {
        var (status, _) = await InvokeMiddleware(new ConflictException("conflict"));
        status.Should().Be(409);
    }

    [Fact]
    public async Task ValidationException_Returns400()
    {
        var failures = new List<ValidationFailure> { new("Field", "Required") };
        var (status, _) = await InvokeMiddleware(new ValidationException(failures));
        status.Should().Be(400);
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
        var (_, body) = await InvokeMiddleware(new NotFoundException("not found"));
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("errorCode").GetString().Should().Be("NOT_FOUND");
    }
}
