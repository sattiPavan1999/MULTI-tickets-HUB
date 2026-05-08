using Bogus;
using FluentValidation.TestHelper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Validators;

namespace IdentityService.Tests.Models;

public class LoginInputTests
{
    private readonly LoginInputValidator _validator = new();
    private static readonly Faker Fake = new();

    private static LoginInput Valid() => new() { Email = Fake.Internet.Email(), Password = "SecurePass1!" };

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Email_InvalidOrEmpty_HasError(string email)
    {
        var input = Valid(); input.Email = email;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Password_Empty_HasError()
    {
        var input = Valid(); input.Password = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidInput_NoErrors()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
