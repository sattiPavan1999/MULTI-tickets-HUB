using FluentValidation.TestHelper;
using TrainService.Core.DTOs;
using TrainService.Core.Validators;

namespace TrainService.Tests.Models;

public class CreateTrainInputTests
{
    private readonly CreateTrainInputValidator _validator = new();

    private static CreateTrainInput ValidInput() => new()
    {
        TrainName = "Rajdhani",
        TrainNumber = "12301",
        Source = "Delhi",
        Destination = "Howrah",
        DepartureTime = DateTime.UtcNow.AddDays(1),
        ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(18),
        Price = 1200m
    };

    [Fact]
    public void EmptyTrainName_HasValidationError()
    {
        var input = ValidInput();
        input.TrainName = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.TrainName);
    }

    [Fact]
    public void EmptyTrainNumber_HasValidationError()
    {
        var input = ValidInput();
        input.TrainNumber = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.TrainNumber);
    }

    [Fact]
    public void EmptySource_HasValidationError()
    {
        var input = ValidInput();
        input.Source = "";
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Source);
    }

    [Fact]
    public void ArrivalTimeBeforeDepartureTime_HasValidationError()
    {
        var input = ValidInput();
        input.ArrivalTime = input.DepartureTime.AddHours(-1);
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.ArrivalTime);
    }

    [Fact]
    public void ZeroPrice_HasValidationError()
    {
        var input = ValidInput();
        input.Price = 0m;
        _validator.TestValidate(input).ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void ValidInput_HasNoErrors()
    {
        _validator.TestValidate(ValidInput()).ShouldNotHaveAnyValidationErrors();
    }
}
