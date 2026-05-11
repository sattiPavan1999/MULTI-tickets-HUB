using FluentValidation.TestHelper;
using MovieService.Core.DTOs;
using MovieService.Core.Validators;

namespace MovieService.Tests.Models;

public class CreateBookingInputTests
{
    private readonly CreateBookingInputValidator _validator = new();

    private static CreateBookingInput Valid() => new()
    {
        ShowtimeId = 1,
        UserId = 42,
        SeatNumbers = [1, 2, 3]
    };

    [Fact]
    public void ValidInput_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ZeroShowtimeId_HasError()
    {
        var input = Valid();
        input.ShowtimeId = 0;
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.ShowtimeId);
    }

    [Fact]
    public void ZeroUserId_HasError()
    {
        var input = Valid();
        input.UserId = 0;
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void EmptySeatNumbers_HasError()
    {
        var input = Valid();
        input.SeatNumbers = [];
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.SeatNumbers);
    }

    [Fact]
    public void DuplicateSeatNumbers_HasError()
    {
        var input = Valid();
        input.SeatNumbers = [1, 1, 2];
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.SeatNumbers);
    }

    [Fact]
    public void MoreThan10Seats_HasError()
    {
        var input = Valid();
        input.SeatNumbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(x => x.SeatNumbers);
    }
}
