using TrainService.DTOs;

namespace TrainService.Tests.DTOs;

public class TrainResponseTests
{
    [Fact]
    public void TrainResponse_Properties_SetCorrectly()
    {
        // Arrange & Act
        var response = new TrainResponse
        {
            Id = 1,
            TrainNumber = "12951",
            TrainName = "Mumbai Rajdhani",
            SourceStation = "Mumbai Central",
            DestinationStation = "New Delhi",
            DepartureTime = "16:55:00",
            ArrivalTime = "08:35:00",
            AvailableSeats = new SeatAvailabilityDto { Sleeper = 72, Ac3Tier = 64, Ac2Tier = 48, Ac1Tier = 24 },
            Fare = new FareDto { Sleeper = 1200.00m, Ac3Tier = 2100.00m, Ac2Tier = 3000.00m, Ac1Tier = 4500.00m }
        };

        // Assert
        Assert.Equal(1, response.Id);
        Assert.Equal("12951", response.TrainNumber);
        Assert.Equal("Mumbai Rajdhani", response.TrainName);
        Assert.Equal("Mumbai Central", response.SourceStation);
        Assert.Equal("New Delhi", response.DestinationStation);
        Assert.Equal("16:55:00", response.DepartureTime);
        Assert.Equal("08:35:00", response.ArrivalTime);
        Assert.NotNull(response.AvailableSeats);
        Assert.NotNull(response.Fare);
    }

    [Fact]
    public void SeatAvailabilityDto_Properties_SetCorrectly()
    {
        // Arrange & Act
        var availability = new SeatAvailabilityDto
        {
            Sleeper = 72,
            Ac3Tier = 64,
            Ac2Tier = 48,
            Ac1Tier = 24
        };

        // Assert
        Assert.Equal(72, availability.Sleeper);
        Assert.Equal(64, availability.Ac3Tier);
        Assert.Equal(48, availability.Ac2Tier);
        Assert.Equal(24, availability.Ac1Tier);
    }

    [Fact]
    public void FareDto_Properties_SetCorrectly()
    {
        // Arrange & Act
        var fare = new FareDto
        {
            Sleeper = 1200.00m,
            Ac3Tier = 2100.00m,
            Ac2Tier = 3000.00m,
            Ac1Tier = 4500.00m
        };

        // Assert
        Assert.Equal(1200.00m, fare.Sleeper);
        Assert.Equal(2100.00m, fare.Ac3Tier);
        Assert.Equal(3000.00m, fare.Ac2Tier);
        Assert.Equal(4500.00m, fare.Ac1Tier);
    }
}
