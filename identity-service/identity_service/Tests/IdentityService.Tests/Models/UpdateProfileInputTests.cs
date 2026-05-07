using System.ComponentModel.DataAnnotations;
using IdentityService.Core.DTOs;

namespace IdentityService.Tests.Models;

public class UpdateProfileInputTests
{
    [Fact]
    public void UpdateProfileInput_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var input = new UpdateProfileInput
        {
            FullName = "John Michael Doe",
            PhoneNumber = "+1234567899",
            Email = "john@example.com"
        };

        // Assert
        Assert.Equal("John Michael Doe", input.FullName);
        Assert.Equal("+1234567899", input.PhoneNumber);
        Assert.Equal("john@example.com", input.Email);
    }

    [Fact]
    public void UpdateProfileInput_WithNullValues_CreatesInstance()
    {
        // Arrange & Act
        var input = new UpdateProfileInput
        {
            FullName = null,
            PhoneNumber = null,
            Email = null
        };

        // Assert
        Assert.Null(input.FullName);
        Assert.Null(input.PhoneNumber);
        Assert.Null(input.Email);
    }

    [Fact]
    public void UpdateProfileInput_OnlyFullName_CreatesInstance()
    {
        // Arrange & Act
        var input = new UpdateProfileInput
        {
            FullName = "Jane Doe"
        };

        // Assert
        Assert.Equal("Jane Doe", input.FullName);
        Assert.Null(input.PhoneNumber);
        Assert.Null(input.Email);
    }

    [Fact]
    public void UpdateProfileInput_OnlyPhoneNumber_CreatesInstance()
    {
        // Arrange & Act
        var input = new UpdateProfileInput
        {
            PhoneNumber = "+9876543210"
        };

        // Assert
        Assert.Null(input.FullName);
        Assert.Equal("+9876543210", input.PhoneNumber);
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("abc123")]
    public void UpdateProfileInput_InvalidPhoneNumber_FailsValidation(string invalidPhone)
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            PhoneNumber = invalidPhone
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("PhoneNumber"));
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+447911123456")]
    public void UpdateProfileInput_ValidPhoneNumber_PassesValidation(string validPhone)
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            PhoneNumber = validPhone
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("+1")]
    [InlineData("12345")]
    public void UpdateProfileInput_PhoneNumberTooShort_FailsValidation(string tooShort)
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            PhoneNumber = tooShort
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("PhoneNumber"));
    }

    [Fact]
    public void UpdateProfileInput_PhoneNumberTooLong_FailsValidation()
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            PhoneNumber = "+1234567890123456789012345"
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("PhoneNumber"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void UpdateProfileInput_InvalidEmail_FailsValidation(string invalidEmail)
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            Email = invalidEmail
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("first.last@sub.example.co.uk")]
    public void UpdateProfileInput_ValidEmail_PassesValidation(string validEmail)
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            Email = validEmail
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateProfileInput_FullNameTooLong_FailsValidation()
    {
        // Arrange
        var input = new UpdateProfileInput
        {
            FullName = new string('a', 256)
        };

        // Act
        var validationResults = ValidateModel(input);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("FullName"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
