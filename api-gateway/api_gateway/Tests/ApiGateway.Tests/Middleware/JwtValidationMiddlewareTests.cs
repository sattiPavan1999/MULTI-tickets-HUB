using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiGateway.Middleware;
using ApiGateway.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Tests.Middleware;

public class JwtValidationMiddlewareTests
{
    private class TestLogger : ILogger<JwtValidationMiddleware>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private readonly JwtSettings _jwtSettings;
    private readonly TestLogger _logger;

    public JwtValidationMiddlewareTests()
    {
        _jwtSettings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVeryLongSecretKeyForTesting123456!",
            TokenExpiryMinutes = 60
        };
        _logger = new TestLogger();
    }

    private string GenerateValidToken(string role = "User", int expiryMinutes = 60)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateExpiredToken(string role = "User")
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Sub, "testuser")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateTokenWithInvalidSignature()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DifferentSecretKey12345678901234567890!"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("role", "User"),
            new Claim(JwtRegisteredClaimNames.Sub, "testuser")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task InvokeAsync_PublicRoute_AuthEndpoint_ShouldAllowWithoutToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/auth";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_HealthEndpoint_ShouldAllowWithoutToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_HealthReadyEndpoint_ShouldAllowWithoutToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health/ready";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_HealthLiveEndpoint_ShouldAllowWithoutToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/health/live";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_RootPath_ShouldAllowWithoutToken()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithValidToken_ShouldAllow()
    {
        // Arrange
        var validToken = GenerateValidToken("User");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = $"Bearer {validToken}";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_MovieRoute_WithValidToken_ShouldAllow()
    {
        // Arrange
        var validToken = GenerateValidToken("User");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/movies";
        context.Request.Headers.Authorization = $"Bearer {validToken}";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AdminRoute_WithAdminToken_ShouldAllow()
    {
        // Arrange
        var adminToken = GenerateValidToken("Admin");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/admin";
        context.Request.Headers.Authorization = $"Bearer {adminToken}";
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AdminRoute_WithUserToken_ShouldReturn403()
    {
        // Arrange
        var userToken = GenerateValidToken("User");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/admin";
        context.Request.Headers.Authorization = $"Bearer {userToken}";
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithExpiredToken_ShouldReturn401()
    {
        // Arrange
        var expiredToken = GenerateExpiredToken();
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = $"Bearer {expiredToken}";
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithInvalidSignature_ShouldReturn401()
    {
        // Arrange
        var invalidToken = GenerateTokenWithInvalidSignature();
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = $"Bearer {invalidToken}";
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithMalformedToken_ShouldReturn401()
    {
        // Arrange
        var malformedToken = "invalid.token";
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = $"Bearer {malformedToken}";
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ProtectedRoute_WithoutBearerPrefix_ShouldReturn401()
    {
        // Arrange
        var validToken = GenerateValidToken("User");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = validToken; // Missing "Bearer " prefix
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/graphql/trains")]
    [InlineData("/graphql/movies")]
    [InlineData("/graphql/admin")]
    public async Task InvokeAsync_ProtectedRoutes_WithoutToken_ShouldReturn401(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/graphql/auth")]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    [InlineData("/")]
    public async Task InvokeAsync_PublicRoutes_WithoutToken_ShouldAllow(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_CaseInsensitivePath_ShouldWork()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/GRAPHQL/AUTH";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AdminRoute_CaseInsensitiveRole_ShouldAllow()
    {
        // Arrange
        var adminToken = GenerateValidToken("admin"); // lowercase
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/admin";
        context.Request.Headers.Authorization = $"Bearer {adminToken}";
        context.Response.Body = new MemoryStream();
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_MultipleValidationCalls_ShouldBeIndependent()
    {
        // Arrange
        var validToken = GenerateValidToken("User");
        var middleware = new JwtValidationMiddleware(
            (ctx) => Task.CompletedTask,
            _jwtSettings,
            _logger
        );

        var context1 = new DefaultHttpContext();
        context1.Request.Path = "/graphql/trains";
        context1.Request.Headers.Authorization = $"Bearer {validToken}";

        var context2 = new DefaultHttpContext();
        context2.Request.Path = "/graphql/movies";
        context2.Request.Headers.Authorization = $"Bearer {validToken}";
        context2.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        Assert.Equal(200, context1.Response.StatusCode);
        Assert.Equal(200, context2.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_EmptyPath_ShouldAllow()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "";
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_TokenWithExtraSpaces_ShouldWork()
    {
        // Arrange
        var validToken = GenerateValidToken("User");
        var context = new DefaultHttpContext();
        context.Request.Path = "/graphql/trains";
        context.Request.Headers.Authorization = $"Bearer   {validToken}   "; // Extra spaces
        var nextCalled = false;
        RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new JwtValidationMiddleware(next, _jwtSettings, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
