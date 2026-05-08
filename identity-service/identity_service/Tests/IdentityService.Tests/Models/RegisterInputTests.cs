using Bogus;
using FluentValidation.TestHelper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Validators;

namespace IdentityService.Tests.Models;

public class RegisterInputTests
{
    private readonly RegisterInputValidator _validator = new();
    private static readonly Faker Fake = new();

    private static RegisterInput Valid() => new()
    {
        Email = Fake.Internet.Email(),
        Password = "SecurePass1!",
        FullName = Fake.Name.FullName(),
        PhoneNumber = "+1234567890"
    };

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Email_InvalidOrEmpty_HasError(string email)
    {
        var input = Valid(); input.Email = email;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Password_TooShort_HasError(string password)
    {
        var input = Valid(); input.Password = password;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void FullName_Empty_HasError()
    {
        var input = Valid(); input.FullName = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData("invalid-phone")]
    [InlineData("abc")]
    public void PhoneNumber_Invalid_HasError(string phone)
    {
        var input = Valid(); input.PhoneNumber = phone;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void ValidInput_NoErrors()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }
}
