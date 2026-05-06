using TrainService.DTOs;

namespace TrainService.Tests.DTOs;

public class SearchTrainInputTests
{
    [Fact]
    public void SearchTrainInput_Properties_SetCorrectly()
    {
        // Arrange & Act
        var input = new SearchTrainInput
        {
            SourceStation = "Mumbai Central",
            DestinationStation = "New Delhi",
            TravelDate = new DateOnly(2026, 6, 15)
        };

        // Assert
        Assert.Equal("Mumbai Central", input.SourceStation);
        Assert.Equal("New Delhi", input.DestinationStation);
        Assert.Equal(new DateOnly(2026, 6, 15), input.TravelDate);
    }

    [Fact]
    public void SearchTrainInput_DefaultValues()
    {
        // Arrange & Act
        var input = new SearchTrainInput();

        // Assert
        Assert.Equal(string.Empty, input.SourceStation);
        Assert.Equal(string.Empty, input.DestinationStation);
        Assert.Equal(default, input.TravelDate);
    }
}
