using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ApiGateway.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.Tests.Integration;

public class FullStackIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JwtSettings _jwtSettings;

    public FullStackIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Get JWT settings from the application
        using var scope = factory.Services.CreateScope();
        _jwtSettings = scope.ServiceProvider.GetRequiredService<JwtSettings>();
    }

    private string GenerateValidToken(string role = "User")
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
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task HealthReadyEndpoint_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ready", content);
    }

    [Fact]
    public async Task HealthLiveEndpoint_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Live", content);
    }

    [Fact]
    public async Task AuthRoute_WithoutToken_ShouldAllowGraphQLRequest()
    {
        // Arrange
        var graphqlRequest = new
        {
            query = "query { hello }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/graphql/auth", content);

        // Assert
        // Note: This will return 502 Bad Gateway because backend service is not running
        // but it should NOT return 401 Unauthorized since this is a public route
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TrainsRoute_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var graphqlRequest = new
        {
            query = "query { trains }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/graphql/trains", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Authorization header missing", responseContent);
    }

    [Fact]
    public async Task MoviesRoute_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var graphqlRequest = new
        {
            query = "query { movies }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/graphql/movies", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminRoute_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var graphqlRequest = new
        {
            query = "query { adminStats }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/graphql/admin", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TrainsRoute_WithValidUserToken_ShouldNotReturn401()
    {
        // Arrange
        var token = GenerateValidToken("User");
        var graphqlRequest = new
        {
            query = "query { trains }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql/trains")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MoviesRoute_WithValidUserToken_ShouldNotReturn401()
    {
        // Arrange
        var token = GenerateValidToken("User");
        var graphqlRequest = new
        {
            query = "query { movies }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql/movies")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminRoute_WithUserToken_ShouldReturn403()
    {
        // Arrange
        var token = GenerateValidToken("User");
        var graphqlRequest = new
        {
            query = "query { adminStats }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql/admin")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Insufficient permissions", responseContent);
    }

    [Fact]
    public async Task AdminRoute_WithAdminToken_ShouldNotReturn401Or403()
    {
        // Arrange
        var token = GenerateValidToken("Admin");
        var graphqlRequest = new
        {
            query = "query { adminStats }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql/admin")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MultipleSequentialRequests_ShouldWorkCorrectly()
    {
        // Arrange
        var token = GenerateValidToken("User");

        // Act & Assert - Health check
        var healthResponse = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);

        // Act & Assert - Public route
        var authRequest = new HttpRequestMessage(HttpMethod.Post, "/graphql/auth")
        {
            Content = new StringContent("{\"query\":\"query{hello}\"}", Encoding.UTF8, "application/json")
        };
        var authResponse = await _client.SendAsync(authRequest);
        Assert.NotEqual(HttpStatusCode.Unauthorized, authResponse.StatusCode);

        // Act & Assert - Protected route with token
        var trainsRequest = new HttpRequestMessage(HttpMethod.Post, "/graphql/trains")
        {
            Content = new StringContent("{\"query\":\"query{trains}\"}", Encoding.UTF8, "application/json")
        };
        trainsRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var trainsResponse = await _client.SendAsync(trainsRequest);
        Assert.NotEqual(HttpStatusCode.Unauthorized, trainsResponse.StatusCode);
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldAllBeHandled()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Send 10 concurrent health check requests
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/health"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - All should return 200 OK
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
    }

    [Fact]
    public async Task InvalidRoute_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/invalid/route");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ErrorResponse_ShouldContainTraceId()
    {
        // Arrange
        var graphqlRequest = new
        {
            query = "query { trains }"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(graphqlRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/graphql/trains", content);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", responseContent);
        Assert.Contains("timestamp", responseContent);
    }
}
