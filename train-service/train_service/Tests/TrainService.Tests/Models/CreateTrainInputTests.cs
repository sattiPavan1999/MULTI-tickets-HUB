using FluentValidation.TestHelper;
using TrainService.Core.DTOs;
using TrainService.Core.Validators;

namespace TrainService.Tests.Models;

public class CreateTrainInputTests
{
    private readonly CreateTrainInputValidator _validator = new();

    [Fact]
    public void EmptyTrainName_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateTrainInput { TrainName = "", TrainNumber = "12301", Source = "A", Destination = "B", DepartureTime = DateTime.UtcNow.AddDays(1) });
        result.ShouldHaveValidationErrorFor(x => x.TrainName);
    }

    [Fact]
    public void EmptyTrainNumber_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateTrainInput { TrainName = "T", TrainNumber = "", Source = "A", Destination = "B", DepartureTime = DateTime.UtcNow.AddDays(1) });
        result.ShouldHaveValidationErrorFor(x => x.TrainNumber);
    }

    [Fact]
    public void EmptySource_HasValidationError()
    {
        var result = _validator.TestValidate(new CreateTrainInput { TrainName = "T", TrainNumber = "12301", Source = "", Destination = "B", DepartureTime = DateTime.UtcNow.AddDays(1) });
        result.ShouldHaveValidationErrorFor(x => x.Source);
    }

    [Fact]
    public void ValidInput_HasNoErrors()
    {
        var result = _validator.TestValidate(new CreateTrainInput { TrainName = "Rajdhani", TrainNumber = "12301", Source = "Delhi", Destination = "Howrah", DepartureTime = DateTime.UtcNow.AddDays(1) });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
