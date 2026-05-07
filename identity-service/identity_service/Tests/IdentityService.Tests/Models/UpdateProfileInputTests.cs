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
            PhoneNumber = "+1234567899"
        };

        // Assert
        Assert.Equal("John Michael Doe", input.FullName);
        Assert.Equal("+1234567899", input.PhoneNumber);
    }

    [Fact]
    public void UpdateProfileInput_WithNullValues_CreatesInstance()
    {
        // Arrange & Act
        var input = new UpdateProfileInput
        {
            FullName = null,
            PhoneNumber = null
        };

        // Assert
        Assert.Null(input.FullName);
        Assert.Null(input.PhoneNumber);
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

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
