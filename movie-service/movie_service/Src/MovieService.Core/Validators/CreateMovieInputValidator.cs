using FluentValidation;
using MovieService.Core.DTOs;

namespace MovieService.Core.Validators;

public class CreateMovieInputValidator : AbstractValidator<CreateMovieInput>
{
    public CreateMovieInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre is required")
            .MaximumLength(100).WithMessage("Genre must not exceed 100 characters");

        RuleFor(x => x.Duration)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");

        RuleFor(x => x.PosterUrl)
            .NotEmpty().WithMessage("PosterUrl is required")
            .MaximumLength(500).WithMessage("PosterUrl must not exceed 500 characters");
    }
}
