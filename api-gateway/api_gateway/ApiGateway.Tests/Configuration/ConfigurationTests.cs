using ApiGateway.Models;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.Tests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_LoadJwtSettings_ShouldSucceed()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:SecretKey"] = "TestSecretKey123!",
            ["JwtSettings:TokenExpiryMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        // Assert
        Assert.NotNull(jwtSettings);
        Assert.Equal("TestIssuer", jwtSettings.Issuer);
        Assert.Equal("TestAudience", jwtSettings.Audience);
        Assert.Equal("TestSecretKey123!", jwtSettings.SecretKey);
        Assert.Equal(60, jwtSettings.TokenExpiryMinutes);
    }

    [Fact]
    public void Configuration_LoadYarpSettings_ShouldContainRoutes()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:identity-route:ClusterId"] = "identity-cluster",
            ["ReverseProxy:Routes:identity-route:Match:Path"] = "/graphql/auth/{**catch-all}",
            ["ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address"] = "http://identity-service:5001"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var routes = configuration.GetSection("ReverseProxy:Routes");
        var clusters = configuration.GetSection("ReverseProxy:Clusters");

        // Assert
        Assert.True(routes.Exists());
        Assert.True(clusters.Exists());
    }

    [Fact]
    public void Configuration_MissingJwtSettings_ShouldReturnNull()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        // Assert
        Assert.Null(jwtSettings);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void Configuration_LoadEnvironmentSpecificSettings_ShouldWork(string environment)
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["ASPNETCORE_ENVIRONMENT"] = environment,
            ["JwtSettings:Issuer"] = $"{environment}Issuer"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        var issuer = configuration["JwtSettings:Issuer"];

        // Assert
        Assert.Equal(environment, env);
        Assert.Equal($"{environment}Issuer", issuer);
    }

    [Fact]
    public void Configuration_OverrideWithEnvironmentVariables_ShouldWork()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "DefaultIssuer",
            ["JwtSettings:SecretKey"] = "DefaultKey"
        };

        var envOverrides = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "OverriddenIssuer" // Environment variables use : not __
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .AddInMemoryCollection(envOverrides) // Later sources override earlier ones
            .Build();

        // Act
        var issuer = configuration["JwtSettings:Issuer"];
        var secretKey = configuration["JwtSettings:SecretKey"];

        // Assert
        Assert.Equal("OverriddenIssuer", issuer);
        Assert.Equal("DefaultKey", secretKey);
    }

    [Fact]
    public void Configuration_AllYarpRoutes_ShouldBeConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["ReverseProxy:Routes:identity-route:ClusterId"] = "identity-cluster",
            ["ReverseProxy:Routes:train-route:ClusterId"] = "train-cluster",
            ["ReverseProxy:Routes:movie-route:ClusterId"] = "movie-cluster",
            ["ReverseProxy:Routes:admin-route:ClusterId"] = "admin-cluster"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var identityRoute = configuration["ReverseProxy:Routes:identity-route:ClusterId"];
        var trainRoute = configuration["ReverseProxy:Routes:train-route:ClusterId"];
        var movieRoute = configuration["ReverseProxy:Routes:movie-route:ClusterId"];
        var adminRoute = configuration["ReverseProxy:Routes:admin-route:ClusterId"];

        // Assert
        Assert.Equal("identity-cluster", identityRoute);
        Assert.Equal("train-cluster", trainRoute);
        Assert.Equal("movie-cluster", movieRoute);
        Assert.Equal("admin-cluster", adminRoute);
    }

    [Fact]
    public void Configuration_AllYarpClusters_ShouldBeConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address"] = "http://identity-service:5001",
            ["ReverseProxy:Clusters:train-cluster:Destinations:destination1:Address"] = "http://train-service:5002",
            ["ReverseProxy:Clusters:movie-cluster:Destinations:destination1:Address"] = "http://movie-service:5003",
            ["ReverseProxy:Clusters:admin-cluster:Destinations:destination1:Address"] = "http://admin-bff:5004"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var identityCluster = configuration["ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address"];
        var trainCluster = configuration["ReverseProxy:Clusters:train-cluster:Destinations:destination1:Address"];
        var movieCluster = configuration["ReverseProxy:Clusters:movie-cluster:Destinations:destination1:Address"];
        var adminCluster = configuration["ReverseProxy:Clusters:admin-cluster:Destinations:destination1:Address"];

        // Assert
        Assert.Equal("http://identity-service:5001", identityCluster);
        Assert.Equal("http://train-service:5002", trainCluster);
        Assert.Equal("http://movie-service:5003", movieCluster);
        Assert.Equal("http://admin-bff:5004", adminCluster);
    }

    [Fact]
    public void Configuration_KestrelEndpoint_ShouldBeConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Kestrel:Endpoints:Http:Url"] = "http://0.0.0.0:5000"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var endpoint = configuration["Kestrel:Endpoints:Http:Url"];

        // Assert
        Assert.Equal("http://0.0.0.0:5000", endpoint);
    }

    [Fact]
    public void Configuration_JwtSettings_ValidationRules_ShouldApply()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = "BookingPlatform",
            ["JwtSettings:Audience"] = "BookingPlatformUsers",
            ["JwtSettings:SecretKey"] = "VeryLongSecretKeyForProduction123!",
            ["JwtSettings:TokenExpiryMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

        // Assert
        Assert.NotNull(jwtSettings);
        Assert.NotEmpty(jwtSettings.Issuer);
        Assert.NotEmpty(jwtSettings.Audience);
        Assert.NotEmpty(jwtSettings.SecretKey);
        Assert.True(jwtSettings.SecretKey.Length >= 32, "Secret key should be at least 32 characters");
        Assert.True(jwtSettings.TokenExpiryMinutes > 0);
    }
}
