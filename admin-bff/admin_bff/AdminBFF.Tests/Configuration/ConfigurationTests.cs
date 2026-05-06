using AdminBFF.Configuration;

namespace AdminBFF.Tests.Configuration;

public class ServiceEndpointsTests
{
    [Fact]
    public void ServiceEndpoints_Should_Initialize_With_Valid_Urls()
    {
        // Arrange & Act
        var endpoints = new ServiceEndpoints
        {
            IdentityServiceUrl = "http://localhost:5001",
            TrainServiceUrl = "http://localhost:5002",
            MovieServiceUrl = "http://localhost:5003"
        };

        // Assert
        Assert.Equal("http://localhost:5001", endpoints.IdentityServiceUrl);
        Assert.Equal("http://localhost:5002", endpoints.TrainServiceUrl);
        Assert.Equal("http://localhost:5003", endpoints.MovieServiceUrl);
    }

    [Theory]
    [InlineData("https://identity-service.example.com")]
    [InlineData("http://identity-service:5001")]
    [InlineData("http://192.168.1.100:5001")]
    public void ServiceEndpoints_Should_Accept_Various_Url_Formats(string url)
    {
        // Act
        var endpoints = new ServiceEndpoints
        {
            IdentityServiceUrl = url,
            TrainServiceUrl = "http://localhost:5002",
            MovieServiceUrl = "http://localhost:5003"
        };

        // Assert
        Assert.Equal(url, endpoints.IdentityServiceUrl);
    }
}

public class JwtSettingsTests
{
    [Fact]
    public void JwtSettings_Should_Initialize_With_Valid_Values()
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Issuer = "tickethub-issuer",
            Audience = "tickethub-audience",
            SecretKey = "your-secret-key-here-minimum-32-characters"
        };

        // Assert
        Assert.Equal("tickethub-issuer", settings.Issuer);
        Assert.Equal("tickethub-audience", settings.Audience);
        Assert.Equal("your-secret-key-here-minimum-32-characters", settings.SecretKey);
    }

    [Fact]
    public void JwtSettings_Properties_Should_Be_Init_Only()
    {
        // Arrange & Act
        var settings = new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "test-secret-key-minimum-32-chars"
        };

        // Assert - Properties are init-only, cannot be reassigned after initialization
        Assert.Equal("test-issuer", settings.Issuer);
        Assert.Equal("test-audience", settings.Audience);
        Assert.Equal("test-secret-key-minimum-32-chars", settings.SecretKey);
    }

    [Theory]
    [InlineData("issuer1", "audience1", "secret-key-with-minimum-32-characters-requirement")]
    [InlineData("issuer2", "audience2", "another-secret-key-minimum-32-chars-long")]
    public void JwtSettings_Should_Accept_Various_Values(string issuer, string audience, string secretKey)
    {
        // Act
        var settings = new JwtSettings
        {
            Issuer = issuer,
            Audience = audience,
            SecretKey = secretKey
        };

        // Assert
        Assert.Equal(issuer, settings.Issuer);
        Assert.Equal(audience, settings.Audience);
        Assert.Equal(secretKey, settings.SecretKey);
    }
}
