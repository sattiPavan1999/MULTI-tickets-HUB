using ApiGateway.Models;

namespace ApiGateway.Tests.Models;

public class JwtSettingsTests
{
    [Fact]
    public void JwtSettings_DefaultConstructor_ShouldInitializeWithEmptyStrings()
    {
        // Arrange & Act
        var settings = new JwtSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(string.Empty, settings.Issuer);
        Assert.Equal(string.Empty, settings.Audience);
        Assert.Equal(string.Empty, settings.SecretKey);
        Assert.Equal(0, settings.TokenExpiryMinutes);
    }

    [Fact]
    public void JwtSettings_SetIssuer_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new JwtSettings();
        var expectedIssuer = "BookingPlatform";

        // Act
        settings.Issuer = expectedIssuer;

        // Assert
        Assert.Equal(expectedIssuer, settings.Issuer);
    }

    [Fact]
    public void JwtSettings_SetAudience_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new JwtSettings();
        var expectedAudience = "BookingPlatformUsers";

        // Act
        settings.Audience = expectedAudience;

        // Assert
        Assert.Equal(expectedAudience, settings.Audience);
    }

    [Fact]
    public void JwtSettings_SetSecretKey_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new JwtSettings();
        var expectedSecretKey = "SuperSecretKey123!";

        // Act
        settings.SecretKey = expectedSecretKey;

        // Assert
        Assert.Equal(expectedSecretKey, settings.SecretKey);
    }

    [Fact]
    public void JwtSettings_SetTokenExpiryMinutes_ShouldSetCorrectly()
    {
        // Arrange
        var settings = new JwtSettings();
        var expectedExpiry = 60;

        // Act
        settings.TokenExpiryMinutes = expectedExpiry;

        // Assert
        Assert.Equal(expectedExpiry, settings.TokenExpiryMinutes);
    }

    [Fact]
    public void JwtSettings_SetAllProperties_ShouldRetainAllValues()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "TestSecretKey",
            TokenExpiryMinutes = 120
        };

        // Assert
        Assert.Equal("TestIssuer", settings.Issuer);
        Assert.Equal("TestAudience", settings.Audience);
        Assert.Equal("TestSecretKey", settings.SecretKey);
        Assert.Equal(120, settings.TokenExpiryMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void JwtSettings_SetIssuer_WithNullOrEmpty_ShouldAccept(string? value)
    {
        // Arrange
        var settings = new JwtSettings();

        // Act
        settings.Issuer = value ?? string.Empty;

        // Assert
        Assert.Equal(value ?? string.Empty, settings.Issuer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(1440)]
    [InlineData(-1)]
    public void JwtSettings_SetTokenExpiryMinutes_WithVariousValues_ShouldAccept(int value)
    {
        // Arrange
        var settings = new JwtSettings();

        // Act
        settings.TokenExpiryMinutes = value;

        // Assert
        Assert.Equal(value, settings.TokenExpiryMinutes);
    }

    [Fact]
    public void JwtSettings_MultipleInstances_ShouldBeIndependent()
    {
        // Arrange & Act
        var settings1 = new JwtSettings { Issuer = "Issuer1", SecretKey = "Key1" };
        var settings2 = new JwtSettings { Issuer = "Issuer2", SecretKey = "Key2" };

        // Assert
        Assert.NotEqual(settings1.Issuer, settings2.Issuer);
        Assert.NotEqual(settings1.SecretKey, settings2.SecretKey);
    }
}
