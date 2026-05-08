using FluentValidation.TestHelper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Validators;

namespace IdentityService.Tests.Models;

public class UpdateProfileInputTests
{
    private readonly UpdateProfileInputValidator _validator = new();

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Email_WhenProvided_InvalidFormat_HasError(string email)
    {
        var input = new UpdateProfileInput { Email = email };
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenNull_NoError()
    {
        _validator.TestValidate(new UpdateProfileInput { Email = null })
            .ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void FullName_WhenProvided_TooLong_HasError()
    {
        var input = new UpdateProfileInput { FullName = new string('a', 256) };
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void PhoneNumber_WhenProvided_Invalid_HasError()
    {
        var input = new UpdateProfileInput { PhoneNumber = "invalid-phone" };
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void AllNull_NoErrors()
    {
        _validator.TestValidate(new UpdateProfileInput()).ShouldNotHaveAnyValidationErrors();
    }
}
