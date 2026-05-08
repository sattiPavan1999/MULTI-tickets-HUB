using FluentValidation;
using IdentityService.Core.DTOs;

namespace IdentityService.Core.Validators;

public class UpdateProfileInputValidator : AbstractValidator<UpdateProfileInput>
{
    public UpdateProfileInputValidator()
    {
        When(x => x.FullName is not null, () =>
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name must not be empty.")
                .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");
        });

        When(x => x.Email is not null, () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email must not be empty.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
        });

        When(x => x.PhoneNumber is not null, () =>
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number must not be empty.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[\d\s\-(). ]+$").WithMessage("Invalid phone number format.");
        });
    }
}
