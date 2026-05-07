using System.ComponentModel.DataAnnotations;
using IdentityService.Core.DTOs;

namespace IdentityService.Tests.Models;

public class ForgotPasswordInputTests
{
    [Fact]
    public void ForgotPasswordInput_WithValidEmail_PassesValidation()
    {
        var input = new ForgotPasswordInput { Email = "user@example.com" };

        var results = ValidateModel(input);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@missing-local.com")]
    public void ForgotPasswordInput_InvalidEmail_FailsValidation(string email)
    {
        var input = new ForgotPasswordInput { Email = email };

        var results = ValidateModel(input);

        Assert.Contains(results, v => v.MemberNames.Contains("Email"));
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }
}
