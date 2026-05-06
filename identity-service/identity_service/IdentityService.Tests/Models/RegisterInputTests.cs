using System.ComponentModel.DataAnnotations;
using IdentityService.Models.DTOs;

namespace IdentityService.Tests.Models;

public class RegisterInputTests
{
    [Fact]
    public void RegisterInput_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var input = new RegisterInput
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        };

        // Assert
        Assert.Equal("test@example.com", input.Email);
        Assert.Equal("SecurePass123!", input.Password);
        Assert.Equal("John Doe", input.FullName);
        Assert.Equal("+1234567890", input.PhoneNumber);
    }

    [Fact]
    public void RegisterInput_EmailRequired_FailsValidation()
    {
        // Arrange
        var input = new RegisterInput
        {
            Email = "",
            Password = "SecurePass123!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
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
    [InlineData("test")]
    public void RegisterInput_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        // Arrange
        var input = new RegisterInput
        {
            Email = invalidEmail,
            Password = "SecurePass123!",
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void RegisterInput_PasswordTooShort_FailsValidation(string shortPassword)
    {
        // Arrange
        var input = new RegisterInput
        {
            Email = "test@example.com",
            Password = shortPassword,
            FullName = "John Doe",
            PhoneNumber = "+1234567890"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RegisterInput_FullNameRequired_FailsValidation(string? fullName)
    {
        // Arrange
        var input = new RegisterInput
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = fullName!,
            PhoneNumber = "+1234567890"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("FullName"));
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("abc")]
    public void RegisterInput_InvalidPhoneNumber_FailsValidation(string invalidPhone)
    {
        // Arrange
        var input = new RegisterInput
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "John Doe",
            PhoneNumber = invalidPhone
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("PhoneNumber"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
