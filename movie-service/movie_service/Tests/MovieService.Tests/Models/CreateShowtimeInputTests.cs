using FluentValidation.TestHelper;
using MovieService.Core.DTOs;
using MovieService.Core.Validators;

namespace MovieService.Tests.Models;

public class CreateShowtimeInputTests
{
    private readonly CreateShowtimeInputValidator _validator = new();

    private static CreateShowtimeInput Valid() => new()
    {
        MovieId = 1,
        ShowDate = "2026-12-25",
        ShowTime = "14:30",
        ScreenNumber = "Screen 1",
        TotalSeats = 50
    };

    [Fact]
    public void ValidInput_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ZeroMovieId_HasError()
    {
        var input = Valid();
        input.MovieId = 0;
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.MovieId);
    }

    [Fact]
    public void EmptyScreenNumber_HasError()
    {
        var input = Valid();
        input.ScreenNumber = "";
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.ScreenNumber);
    }

    [Fact]
    public void ZeroTotalSeats_HasError()
    {
        var input = Valid();
        input.TotalSeats = 0;
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.TotalSeats);
    }
}
