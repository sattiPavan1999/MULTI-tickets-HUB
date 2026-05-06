using IdentityService.Models.GraphQL;

namespace IdentityService.Tests.Models;

public class UserTypeTests
{
    [Fact]
    public void UserType_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var userType = new UserType
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, userType.Id);
        Assert.Equal("test@example.com", userType.Email);
        Assert.Equal("John Doe", userType.FullName);
        Assert.Equal("+1234567890", userType.PhoneNumber);
        Assert.Equal("User", userType.Role);
        Assert.NotNull(userType.CreatedAt);
    }

    [Fact]
    public void UserType_CreatedAtNullable_AcceptsNull()
    {
        // Arrange & Act
        var userType = new UserType
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = "User",
            CreatedAt = null
        };

        // Assert
        Assert.Null(userType.CreatedAt);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public void UserType_SupportsRoles_StoresCorrectly(string role)
    {
        // Arrange & Act
        var userType = new UserType
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "John Doe",
            PhoneNumber = "+1234567890",
            Role = role
        };

        // Assert
        Assert.Equal(role, userType.Role);
    }
}
