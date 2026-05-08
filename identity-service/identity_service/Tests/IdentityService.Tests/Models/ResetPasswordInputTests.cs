using FluentValidation.TestHelper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Validators;

namespace IdentityService.Tests.Models;

public class ResetPasswordInputTests
{
    private readonly ResetPasswordInputValidator _validator = new();

    [Fact]
    public void Token_Empty_HasError()
    {
        _validator.TestValidate(new ResetPasswordInput { Token = "", NewPassword = "Password1!" })
            .ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void NewPassword_TooShort_HasError(string password)
    {
        _validator.TestValidate(new ResetPasswordInput { Token = "valid-token", NewPassword = password })
            .ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ValidInput_NoErrors()
    {
        _validator.TestValidate(new ResetPasswordInput { Token = "abc", NewPassword = "Password1!" })
            .ShouldNotHaveAnyValidationErrors();
    }
}
