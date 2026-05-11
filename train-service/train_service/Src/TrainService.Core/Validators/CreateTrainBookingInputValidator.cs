using FluentValidation;
using TrainService.Core.DTOs;

namespace TrainService.Core.Validators;

public class CreateTrainBookingInputValidator : AbstractValidator<CreateTrainBookingInput>
{
    public CreateTrainBookingInputValidator()
    {
        RuleFor(x => x.TrainId).GreaterThan(0).WithMessage("TrainId must be greater than 0");
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0");
        RuleFor(x => x.TravelDate)
            .NotEmpty().WithMessage("TravelDate is required")
            .Must(BeValidDateFormat).WithMessage("TravelDate must be in YYYY-MM-DD format")
            .Must(BeTodayOrFuture).WithMessage("TravelDate must be today or a future date");
        RuleFor(x => x.PassengerName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PassengerAge)
            .InclusiveBetween(1, 120).WithMessage("PassengerAge must be between 1 and 120");
        RuleFor(x => x.NumberOfSeats)
            .InclusiveBetween(1, 6).WithMessage("NumberOfSeats must be between 1 and 6");
    }

    private static bool BeValidDateFormat(string date)
        => DateOnly.TryParseExact(date, "yyyy-MM-dd", out _);

    private static bool BeTodayOrFuture(string date)
        => DateOnly.TryParseExact(date, "yyyy-MM-dd", out var d) && d >= DateOnly.FromDateTime(DateTime.UtcNow);
}
