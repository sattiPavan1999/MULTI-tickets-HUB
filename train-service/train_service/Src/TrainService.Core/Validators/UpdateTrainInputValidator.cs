using FluentValidation;
using TrainService.Core.DTOs;

namespace TrainService.Core.Validators;

public class UpdateTrainInputValidator : AbstractValidator<UpdateTrainInput>
{
    public UpdateTrainInputValidator()
    {
        When(x => x.TrainName is not null, () => RuleFor(x => x.TrainName!).NotEmpty().MaximumLength(255));
        When(x => x.TrainNumber is not null, () => RuleFor(x => x.TrainNumber!).NotEmpty().MaximumLength(50));
        When(x => x.Source is not null, () => RuleFor(x => x.Source!).NotEmpty().MaximumLength(255));
        When(x => x.Destination is not null, () => RuleFor(x => x.Destination!).NotEmpty().MaximumLength(255));
        When(x => x.Price.HasValue, () => RuleFor(x => x.Price!.Value).GreaterThan(0).WithMessage("Price must be greater than 0"));
    }
}
