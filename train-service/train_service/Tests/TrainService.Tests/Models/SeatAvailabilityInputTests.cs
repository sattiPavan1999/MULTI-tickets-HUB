using FluentValidation.TestHelper;
using TrainService.Core.DTOs;
using TrainService.Core.Validators;

namespace TrainService.Tests.Models;

public class SeatAvailabilityInputTests
{
    private readonly SeatAvailabilityInputValidator _validator = new();

    [Fact]
    public void NegativeSeats_HasValidationError()
    {
        var result = _validator.TestValidate(new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = -1 });
        result.ShouldHaveValidationErrorFor(x => x.AvailableSeats);
    }

    [Fact]
    public void ZeroSeats_IsValid()
    {
        var result = _validator.TestValidate(new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 0 });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PositiveSeats_IsValid()
    {
        var result = _validator.TestValidate(new SeatAvailabilityInput { Date = DateOnly.FromDateTime(DateTime.UtcNow), AvailableSeats = 100 });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
