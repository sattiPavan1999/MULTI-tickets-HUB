using FluentValidation;
using TrainService.Core.DTOs;

namespace TrainService.Core.Validators;

public class SeatAvailabilityInputValidator : AbstractValidator<SeatAvailabilityInput>
{
    public SeatAvailabilityInputValidator()
    {
        RuleFor(x => x.AvailableSeats).GreaterThanOrEqualTo(0).WithMessage("AvailableSeats must be 0 or greater");
    }
}
