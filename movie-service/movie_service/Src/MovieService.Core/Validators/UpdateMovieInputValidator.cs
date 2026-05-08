using FluentValidation;
using MovieService.Core.DTOs;

namespace MovieService.Core.Validators;

public class UpdateMovieInputValidator : AbstractValidator<UpdateMovieInput>
{
    public UpdateMovieInputValidator()
    {
        When(x => x.Title is not null, () =>
            RuleFor(x => x.Title!)
                .NotEmpty().WithMessage("Title must not be empty")
                .MaximumLength(255).WithMessage("Title must not exceed 255 characters"));

        When(x => x.Genre is not null, () =>
            RuleFor(x => x.Genre!)
                .NotEmpty().WithMessage("Genre must not be empty")
                .MaximumLength(100).WithMessage("Genre must not exceed 100 characters"));

        When(x => x.Duration.HasValue, () =>
            RuleFor(x => x.Duration!.Value)
                .GreaterThan(0).WithMessage("Duration must be greater than 0"));

        When(x => x.PosterUrl is not null, () =>
            RuleFor(x => x.PosterUrl!)
                .NotEmpty().WithMessage("PosterUrl must not be empty")
                .MaximumLength(500).WithMessage("PosterUrl must not exceed 500 characters"));
    }
}
