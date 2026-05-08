using FluentValidation.TestHelper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Validators;

namespace IdentityService.Tests.Models;

public class ForgotPasswordInputTests
{
    private readonly ForgotPasswordInputValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Email_InvalidOrEmpty_HasError(string email)
    {
        _validator.TestValidate(new ForgotPasswordInput { Email = email })
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ValidEmail_NoErrors()
    {
        _validator.TestValidate(new ForgotPasswordInput { Email = "user@example.com" })
            .ShouldNotHaveAnyValidationErrors();
    }
}
