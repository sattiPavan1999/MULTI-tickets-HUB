using FluentValidation.TestHelper;
using MovieService.Core.DTOs;
using MovieService.Core.Validators;

namespace MovieService.Tests.Models;

public class UpdateMovieInputTests
{
    private readonly UpdateMovieInputValidator _validator = new();

    [Fact]
    public void AllNullFields_IsValid()
    {
        var result = _validator.TestValidate(new UpdateMovieInput());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTitle_WhenProvided_HasValidationError()
    {
        var result = _validator.TestValidate(new UpdateMovieInput { Title = "" });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void ValidTitle_WhenProvided_NoError()
    {
        var result = _validator.TestValidate(new UpdateMovieInput { Title = "New Title" });
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void ZeroDuration_WhenProvided_HasValidationError()
    {
        var result = _validator.TestValidate(new UpdateMovieInput { Duration = 0 });
        result.ShouldHaveValidationErrorFor(x => x.Duration!.Value);
    }

    [Fact]
    public void PositiveDuration_WhenProvided_NoError()
    {
        var result = _validator.TestValidate(new UpdateMovieInput { Duration = 90 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
