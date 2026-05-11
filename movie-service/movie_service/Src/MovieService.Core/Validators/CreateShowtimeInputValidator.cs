using FluentValidation;
using MovieService.Core.DTOs;

namespace MovieService.Core.Validators;

public class CreateShowtimeInputValidator : AbstractValidator<CreateShowtimeInput>
{
    public CreateShowtimeInputValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0).WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.ShowDate)
            .NotEmpty().WithMessage("Show date is required.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Show date must be in YYYY-MM-DD format.");

        RuleFor(x => x.ShowTime)
            .NotEmpty().WithMessage("Show time is required.")
            .Matches(@"^\d{2}:\d{2}(:\d{2})?$").WithMessage("Show time must be in HH:mm or HH:mm:ss format.");

        RuleFor(x => x.ScreenNumber)
            .NotEmpty().WithMessage("Screen number is required.")
            .MaximumLength(100).WithMessage("Screen number must not exceed 100 characters.");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Total seats must be greater than 0.");
    }
}
