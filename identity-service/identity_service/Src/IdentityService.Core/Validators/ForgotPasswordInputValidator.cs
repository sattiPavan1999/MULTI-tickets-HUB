using FluentValidation;
using IdentityService.Core.DTOs;

namespace IdentityService.Core.Validators;

public class ForgotPasswordInputValidator : AbstractValidator<ForgotPasswordInput>
{
    public ForgotPasswordInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
