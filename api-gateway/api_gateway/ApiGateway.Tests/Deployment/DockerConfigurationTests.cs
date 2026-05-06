namespace ApiGateway.Tests.Deployment;

public class DockerConfigurationTests
{
    [Fact]
    public void Dockerfile_ShouldExist()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");

        // Act & Assert
        Assert.True(File.Exists(dockerfilePath), "Dockerfile should exist in project root");
    }

    [Fact]
    public void Dockerfile_ShouldContainMultiStageSetup()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build", content);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime", content);
    }

    [Fact]
    public void Dockerfile_ShouldContainNonRootUser()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("groupadd", content);
        Assert.Contains("useradd", content);
        Assert.Contains("USER appuser", content);
    }

    [Fact]
    public void Dockerfile_ShouldExposePort5000()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("EXPOSE 5000", content);
    }

    [Fact]
    public void Dockerfile_ShouldContainHealthCheck()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("HEALTHCHECK", content);
        Assert.Contains("/health", content);
    }

    [Fact]
    public void Dockerfile_ShouldSetAspNetCoreEnvironment()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("ASPNETCORE_URLS", content);
        Assert.Contains("ASPNETCORE_ENVIRONMENT", content);
    }

    [Fact]
    public void DockerCompose_ShouldExist()
    {
        // Arrange
        var dockerComposePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "docker-compose.yml");

        // Act & Assert
        Assert.True(File.Exists(dockerComposePath), "docker-compose.yml should exist in project root");
    }

    [Fact]
    public void DockerCompose_ShouldDefineApiGatewayService()
    {
        // Arrange
        var dockerComposePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "docker-compose.yml");
        var content = File.ReadAllText(dockerComposePath);

        // Act & Assert
        Assert.Contains("api-gateway", content);
        Assert.Contains("5000:5000", content);
    }

    [Fact]
    public void DockerCompose_ShouldContainJwtSettings()
    {
        // Arrange
        var dockerComposePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "docker-compose.yml");
        var content = File.ReadAllText(dockerComposePath);

        // Act & Assert
        Assert.Contains("JwtSettings__Issuer", content);
        Assert.Contains("JwtSettings__Audience", content);
        Assert.Contains("JwtSettings__SecretKey", content);
    }

    [Fact]
    public void DockerCompose_ShouldContainHealthCheck()
    {
        // Arrange
        var dockerComposePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "docker-compose.yml");
        var content = File.ReadAllText(dockerComposePath);

        // Act & Assert
        Assert.Contains("healthcheck", content);
    }

    [Fact]
    public void DockerCompose_ShouldDefineNetwork()
    {
        // Arrange
        var dockerComposePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "docker-compose.yml");
        var content = File.ReadAllText(dockerComposePath);

        // Act & Assert
        Assert.Contains("networks:", content);
        Assert.Contains("booking-network", content);
    }

    [Fact]
    public void DockerIgnore_ShouldExist()
    {
        // Arrange
        var dockerIgnorePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", ".dockerignore");

        // Act & Assert
        Assert.True(File.Exists(dockerIgnorePath), ".dockerignore should exist");
    }

    [Fact]
    public void DockerIgnore_ShouldIgnoreBinAndObj()
    {
        // Arrange
        var dockerIgnorePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", ".dockerignore");
        var content = File.ReadAllText(dockerIgnorePath);

        // Act & Assert
        Assert.Contains("**/bin", content);
        Assert.Contains("**/obj", content);
    }

    [Fact]
    public void Dockerfile_EntryPoint_ShouldBeDotNetApiGateway()
    {
        // Arrange
        var dockerfilePath = Path.Combine(Directory.GetCurrentDirectory(), "../../../..", "Dockerfile");
        var content = File.ReadAllText(dockerfilePath);

        // Act & Assert
        Assert.Contains("ENTRYPOINT", content);
        Assert.Contains("ApiGateway.dll", content);
    }
}
