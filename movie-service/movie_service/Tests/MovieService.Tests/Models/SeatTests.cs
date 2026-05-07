using MovieService.Core.Models;

namespace MovieService.Tests.Models;

public class SeatTests
{
    [Fact]
    public void Seat_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var seat = new Seat
        {
            Id = 1,
            ScreenId = 5,
            RowLabel = "A",
            SeatNumber = 10,
            Category = "Regular",
            Price = 200.00m
        };

        // Assert
        Assert.Equal(1, seat.Id);
        Assert.Equal(5, seat.ScreenId);
        Assert.Equal("A", seat.RowLabel);
        Assert.Equal(10, seat.SeatNumber);
        Assert.Equal("Regular", seat.Category);
        Assert.Equal(200.00m, seat.Price);
    }

    [Theory]
    [InlineData("Regular", 200.00)]
    [InlineData("Premium", 350.00)]
    [InlineData("Recliner", 500.00)]
    public void Seat_Should_HandleDifferentCategoriesAndPrices(string category, decimal price)
    {
        // Arrange & Act
        var seat = new Seat
        {
            ScreenId = 1,
            RowLabel = "A",
            SeatNumber = 1,
            Category = category,
            Price = price
        };

        // Assert
        Assert.Equal(category, seat.Category);
        Assert.Equal(price, seat.Price);
    }

    [Fact]
    public void Seat_Should_HandleDifferentRows()
    {
        // Arrange & Act
        var seats = new[]
        {
            new Seat { ScreenId = 1, RowLabel = "A", SeatNumber = 1, Category = "Regular", Price = 200m },
            new Seat { ScreenId = 1, RowLabel = "B", SeatNumber = 1, Category = "Premium", Price = 350m },
            new Seat { ScreenId = 1, RowLabel = "F", SeatNumber = 1, Category = "Recliner", Price = 500m }
        };

        // Assert
        Assert.Equal("A", seats[0].RowLabel);
        Assert.Equal("B", seats[1].RowLabel);
        Assert.Equal("F", seats[2].RowLabel);
    }
}
