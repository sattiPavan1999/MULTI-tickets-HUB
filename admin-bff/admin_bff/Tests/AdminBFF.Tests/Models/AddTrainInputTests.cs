using AdminBFF.Core.Models;

namespace AdminBFF.Tests.Models;

public class AddTrainInputTests
{
    [Fact]
    public void AddTrainInput_Should_Initialize_With_Valid_Values()
    {
        // Arrange & Act
        var input = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "CityA",
            DestinationStation = "CityB",
            DepartureTime = "10:00",
            ArrivalTime = "18:00",
            TotalSeats = new Dictionary<string, int>
            {
                ["Sleeper"] = 200,
                ["AC3"] = 100,
                ["AC2"] = 50,
                ["AC1"] = 20
            }
        };

        // Assert
        Assert.Equal("12345", input.TrainNumber);
        Assert.Equal("Express", input.TrainName);
        Assert.Equal("CityA", input.SourceStation);
        Assert.Equal("CityB", input.DestinationStation);
        Assert.Equal("10:00", input.DepartureTime);
        Assert.Equal("18:00", input.ArrivalTime);
        Assert.Equal(4, input.TotalSeats.Count);
        Assert.Equal(200, input.TotalSeats["Sleeper"]);
        Assert.Equal(100, input.TotalSeats["AC3"]);
        Assert.Equal(50, input.TotalSeats["AC2"]);
        Assert.Equal(20, input.TotalSeats["AC1"]);
    }

    [Fact]
    public void AddTrainInput_Should_Be_Immutable()
    {
        // Arrange
        var original = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "CityA",
            DestinationStation = "CityB",
            DepartureTime = "10:00",
            ArrivalTime = "18:00",
            TotalSeats = new Dictionary<string, int> { ["Sleeper"] = 200 }
        };

        // Act
        var modified = original with { TrainName = "Modified Express" };

        // Assert
        Assert.Equal("Express", original.TrainName);
        Assert.Equal("Modified Express", modified.TrainName);
    }

    [Fact]
    public void AddTrainInput_Should_Handle_Empty_Seats_Dictionary()
    {
        // Arrange & Act
        var input = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "CityA",
            DestinationStation = "CityB",
            DepartureTime = "10:00",
            ArrivalTime = "18:00",
            TotalSeats = new Dictionary<string, int>()
        };

        // Assert
        Assert.Empty(input.TotalSeats);
    }

    [Theory]
    [InlineData("08:00", "20:00")]
    [InlineData("00:00", "23:59")]
    [InlineData("12:30", "14:45")]
    public void AddTrainInput_Should_Accept_Various_Time_Formats(string departure, string arrival)
    {
        // Act
        var input = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "CityA",
            DestinationStation = "CityB",
            DepartureTime = departure,
            ArrivalTime = arrival,
            TotalSeats = new Dictionary<string, int> { ["Sleeper"] = 100 }
        };

        // Assert
        Assert.Equal(departure, input.DepartureTime);
        Assert.Equal(arrival, input.ArrivalTime);
    }
}
