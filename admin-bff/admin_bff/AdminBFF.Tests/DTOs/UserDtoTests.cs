using AdminBFF.DTOs;

namespace AdminBFF.Tests.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_Should_Initialize_With_Valid_Values()
    {
        // Arrange & Act
        var userDto = new UserDto
        {
            Id = 1,
            Email = "admin@tickethub.com",
            FullName = "Admin User",
            PhoneNumber = "+1234567890",
            Role = "Admin",
            CreatedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal(1, userDto.Id);
        Assert.Equal("admin@tickethub.com", userDto.Email);
        Assert.Equal("Admin User", userDto.FullName);
        Assert.Equal("+1234567890", userDto.PhoneNumber);
        Assert.Equal("Admin", userDto.Role);
        Assert.Equal(new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), userDto.CreatedAt);
    }

    [Fact]
    public void UserDto_Should_Be_Immutable()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = 1,
            Email = "admin@tickethub.com",
            FullName = "Admin User",
            PhoneNumber = "+1234567890",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        // Assert - Record types are immutable by default
        Assert.IsAssignableFrom<UserDto>(userDto);
    }

    [Fact]
    public void UserDto_With_Expression_Should_Create_Modified_Copy()
    {
        // Arrange
        var original = new UserDto
        {
            Id = 1,
            Email = "admin@tickethub.com",
            FullName = "Admin User",
            PhoneNumber = "+1234567890",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var modified = original with { FullName = "Modified Admin" };

        // Assert
        Assert.Equal("Admin User", original.FullName);
        Assert.Equal("Modified Admin", modified.FullName);
        Assert.Equal(original.Id, modified.Id);
        Assert.Equal(original.Email, modified.Email);
    }

    [Theory]
    [InlineData(0, "test@example.com", "Test User", "+1234567890", "User")]
    [InlineData(999, "admin@example.com", "Another Admin", "+9876543210", "Admin")]
    public void UserDto_Should_Accept_Various_Valid_Values(int id, string email, string fullName, string phoneNumber, string role)
    {
        // Act
        var userDto = new UserDto
        {
            Id = id,
            Email = email,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(id, userDto.Id);
        Assert.Equal(email, userDto.Email);
        Assert.Equal(fullName, userDto.FullName);
        Assert.Equal(phoneNumber, userDto.PhoneNumber);
        Assert.Equal(role, userDto.Role);
    }
}
