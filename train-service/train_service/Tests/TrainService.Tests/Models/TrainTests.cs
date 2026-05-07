using TrainService.Core.Models;

namespace TrainService.Tests.Models;

public class TrainTests
{
    [Fact]
    public void Train_Properties_SetCorrectly()
    {
        // Arrange & Act
        var train = new Train
        {
            Id = 1,
            TrainNumber = "12951",
            TrainName = "Mumbai Rajdhani",
            SourceStation = "Mumbai Central",
            DestinationStation = "New Delhi",
            DepartureTime = new TimeSpan(16, 55, 0),
            ArrivalTime = new TimeSpan(8, 35, 0),
            TotalSeats = "{\"sleeper\": 72}",
            Fares = "{\"sleeper\": 1200.00}"
        };

        // Assert
        Assert.Equal(1, train.Id);
        Assert.Equal("12951", train.TrainNumber);
        Assert.Equal("Mumbai Rajdhani", train.TrainName);
        Assert.Equal("Mumbai Central", train.SourceStation);
        Assert.Equal("New Delhi", train.DestinationStation);
        Assert.Equal(new TimeSpan(16, 55, 0), train.DepartureTime);
        Assert.Equal(new TimeSpan(8, 35, 0), train.ArrivalTime);
        Assert.Equal("{\"sleeper\": 72}", train.TotalSeats);
        Assert.Equal("{\"sleeper\": 1200.00}", train.Fares);
    }

    [Fact]
    public void Train_DefaultValues()
    {
        // Arrange & Act
        var train = new Train();

        // Assert
        Assert.Equal(0, train.Id);
        Assert.Equal(string.Empty, train.TrainNumber);
        Assert.Equal(string.Empty, train.TrainName);
        Assert.Equal(string.Empty, train.SourceStation);
        Assert.Equal(string.Empty, train.DestinationStation);
        Assert.Equal(TimeSpan.Zero, train.DepartureTime);
        Assert.Equal(TimeSpan.Zero, train.ArrivalTime);
        Assert.Equal(string.Empty, train.TotalSeats);
        Assert.Equal(string.Empty, train.Fares);
        Assert.NotNull(train.Bookings);
        Assert.Empty(train.Bookings);
    }

    [Fact]
    public void Train_Bookings_CollectionInitialized()
    {
        // Arrange & Act
        var train = new Train();

        // Assert
        Assert.NotNull(train.Bookings);
        Assert.IsAssignableFrom<ICollection<TrainBooking>>(train.Bookings);
    }
}
