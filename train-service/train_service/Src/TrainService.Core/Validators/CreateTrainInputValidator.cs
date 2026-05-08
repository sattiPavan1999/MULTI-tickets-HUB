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
    }
}
