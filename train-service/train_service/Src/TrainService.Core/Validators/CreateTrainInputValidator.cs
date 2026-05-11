using FluentValidation;
using TrainService.Core.DTOs;

namespace TrainService.Core.Validators;

public class CreateTrainInputValidator : AbstractValidator<CreateTrainInput>
{
    public CreateTrainInputValidator()
    {
        RuleFor(x => x.TrainName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TrainNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DepartureTime).GreaterThan(DateTime.MinValue).WithMessage("DepartureTime is required");
        RuleFor(x => x.ArrivalTime)
            .GreaterThan(DateTime.MinValue).WithMessage("ArrivalTime is required")
            .GreaterThan(x => x.DepartureTime).WithMessage("ArrivalTime must be after DepartureTime");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}
