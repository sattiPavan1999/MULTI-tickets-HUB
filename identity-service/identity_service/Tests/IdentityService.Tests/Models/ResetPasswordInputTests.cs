using System.ComponentModel.DataAnnotations;
using IdentityService.Core.DTOs;

namespace IdentityService.Tests.Models;

public class ResetPasswordInputTests
{
    [Fact]
    public void ResetPasswordInput_WithValidData_PassesValidation()
    {
        var input = new ResetPasswordInput
        {
            Token = "abc123",
            NewPassword = "NewSecurePass1!"
        };

        var results = ValidateModel(input);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    public void ResetPasswordInput_MissingToken_FailsValidation(string token)
    {
        var input = new ResetPasswordInput
        {
            Token = token,
            NewPassword = "NewSecurePass1!"
        };

        var results = ValidateModel(input);

        Assert.Contains(results, v => v.MemberNames.Contains("Token"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void ResetPasswordInput_PasswordTooShort_FailsValidation(string password)
    {
        var input = new ResetPasswordInput
        {
            Token = "valid-token",
            NewPassword = password
        };

        var results = ValidateModel(input);

        Assert.Contains(results, v => v.MemberNames.Contains("NewPassword"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }
}
