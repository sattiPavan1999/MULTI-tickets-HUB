using FluentValidation;
using MovieService.Core.DTOs;

namespace MovieService.Core.Validators;

public class CreateBookingInputValidator : AbstractValidator<CreateBookingInput>
{
    public CreateBookingInputValidator()
    {
        RuleFor(x => x.ShowtimeId)
            .GreaterThan(0).WithMessage("ShowtimeId must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.SeatNumbers)
            .NotEmpty().WithMessage("At least one seat number is required.")
            .Must(s => s.All(n => n > 0)).WithMessage("All seat numbers must be greater than 0.")
            .Must(s => s.Distinct().Count() == s.Count).WithMessage("Seat numbers must be unique.")
            .Must(s => s.Count <= 10).WithMessage("Cannot book more than 10 seats at once.");
    }
}
