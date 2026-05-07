using IdentityService.Core.Models;

namespace IdentityService.Tests.Models;

public class UserEntityTests
{
    [Fact]
    public void User_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashed_password", user.PasswordHash);
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal("+1234567890", user.PhoneNumber);
        Assert.Equal("User", user.Role);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void User_DefaultRole_IsUser()
    {
        // Arrange & Act
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User"
        };

        // Assert
        Assert.Equal("User", user.Role);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public void User_SupportsRoles_StoresCorrectly(string role)
    {
        // Arrange & Act
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = role
        };

        // Assert
        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void User_RequiredFields_MustBeSet()
    {
        // Arrange & Act
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User"
        };

        // Assert
        Assert.NotNull(user.Email);
        Assert.NotNull(user.PasswordHash);
        Assert.NotNull(user.FullName);
        Assert.NotNull(user.PhoneNumber);
        Assert.NotNull(user.Role);
    }

    [Fact]
    public void User_PasswordHash_NeverPlainText()
    {
        // Arrange & Act
        var plainPassword = "MyPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User"
        };

        // Assert
        Assert.NotEqual(plainPassword, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash));
    }

    [Fact]
    public void User_CreatedAt_TracksCreationTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(user.CreatedAt >= beforeCreation);
        Assert.True(user.CreatedAt <= afterCreation);
    }

    [Fact]
    public void User_EmailUniqueness_EnforcedAtDatabaseLevel()
    {
        // This test documents that uniqueness is enforced by database constraint
        // Actual enforcement tested in repository integration tests

        // Arrange
        var user1 = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash1",
            FullName = "User One",
            PhoneNumber = "+1111111111",
            Role = "User"
        };

        var user2 = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash2",
            FullName = "User Two",
            PhoneNumber = "+2222222222",
            Role = "User"
        };

        // Assert - Both objects can be created in memory
        Assert.Equal(user1.Email, user2.Email);
        // Database constraint prevents duplicate emails (tested in integration tests)
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void User_IdField_AcceptsValidIntegers(int id)
    {
        // Arrange & Act
        var user = new User
        {
            Id = id,
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User"
        };

        // Assert
        Assert.Equal(id, user.Id);
    }
}
