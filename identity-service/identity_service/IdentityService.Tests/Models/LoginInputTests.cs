using System.ComponentModel.DataAnnotations;
using IdentityService.Models.DTOs;

namespace IdentityService.Tests.Models;

public class LoginInputTests
{
    [Fact]
    public void LoginInput_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var input = new LoginInput
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        // Assert
        Assert.Equal("test@example.com", input.Email);
        Assert.Equal("SecurePass123!", input.Password);
    }

    [Fact]
    public void LoginInput_EmailRequired_FailsValidation()
    {
        // Arrange
        var input = new LoginInput
        {
            Email = "",
            Password = "SecurePass123!"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void LoginInput_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        // Arrange
        var input = new LoginInput
        {
            Email = invalidEmail,
            Password = "SecurePass123!"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginInput_PasswordRequired_FailsValidation()
    {
        // Arrange
        var input = new LoginInput
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
