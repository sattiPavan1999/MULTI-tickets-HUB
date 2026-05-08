using FluentValidation.TestHelper;
using MovieService.Core.DTOs;
using MovieService.Core.Validators;

namespace MovieService.Tests.Models;

public class CreateMovieInputTests
{
    private readonly CreateMovieInputValidator _validator = new();

    [Fact]
    public void EmptyTitle_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "", Genre = "G", Duration = 1, PosterUrl = "https://example.com/p.jpg" });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TitleOver255Chars_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = new string('A', 256), Genre = "G", Duration = 1, PosterUrl = "https://example.com/p.jpg" });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void EmptyGenre_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "T", Genre = "", Duration = 1, PosterUrl = "https://example.com/p.jpg" });
        result.ShouldHaveValidationErrorFor(x => x.Genre);
    }

    [Fact]
    public void ZeroDuration_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "T", Genre = "G", Duration = 0, PosterUrl = "https://example.com/p.jpg" });
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void NegativeDuration_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "T", Genre = "G", Duration = -1, PosterUrl = "https://example.com/p.jpg" });
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void EmptyPosterUrl_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "T", Genre = "G", Duration = 1, PosterUrl = "" });
        result.ShouldHaveValidationErrorFor(x => x.PosterUrl);
    }

    [Fact]
    public void ValidInput_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new CreateMovieInput { Title = "Inception", Genre = "Sci-Fi", Duration = 148, PosterUrl = "https://example.com/poster.jpg" });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
