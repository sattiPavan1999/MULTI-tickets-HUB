using FluentValidation.TestHelper;
using TrainService.Core.DTOs;
using TrainService.Core.Validators;

namespace TrainService.Tests.Models;

public class CreateTrainBookingInputTests
{
    private readonly CreateTrainBookingInputValidator _validator = new();

    private static CreateTrainBookingInput ValidInput() => new()
    {
        TrainId = 1,
        UserId = 1,
        TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
        PassengerName = "John Doe",
        PassengerAge = 30,
        NumberOfSeats = 2
    };

    [Fact]
    public void ValidInput_HasNoErrors()
        => _validator.TestValidate(ValidInput()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ZeroTrainId_HasValidationError()
    {
        var input = ValidInput();
        input.TrainId = 0;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.TrainId);
    }

    [Fact]
    public void ZeroUserId_HasValidationError()
    {
        var input = ValidInput();
        input.UserId = 0;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void EmptyPassengerName_HasValidationError()
    {
        var input = ValidInput();
        input.PassengerName = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.PassengerName);
    }

    [Fact]
    public void PassengerAgeZero_HasValidationError()
    {
        var input = ValidInput();
        input.PassengerAge = 0;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.PassengerAge);
    }

    [Fact]
    public void PassengerAge121_HasValidationError()
    {
        var input = ValidInput();
        input.PassengerAge = 121;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.PassengerAge);
    }

    [Fact]
    public void NumberOfSeatsZero_HasValidationError()
    {
        var input = ValidInput();
        input.NumberOfSeats = 0;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.NumberOfSeats);
    }

    [Fact]
    public void NumberOfSeats7_HasValidationError()
    {
        var input = ValidInput();
        input.NumberOfSeats = 7;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.NumberOfSeats);
    }

    [Fact]
    public void InvalidDateFormat_HasValidationError()
    {
        var input = ValidInput();
        input.TravelDate = "01/01/2026";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.TravelDate);
    }

    [Fact]
    public void PastDate_HasValidationError()
    {
        var input = ValidInput();
        input.TravelDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1).ToString("yyyy-MM-dd");
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.TravelDate);
    }
}
