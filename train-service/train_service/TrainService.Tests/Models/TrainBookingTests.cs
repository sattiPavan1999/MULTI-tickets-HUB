using TrainService.Models;

namespace TrainService.Tests.Models;

public class TrainBookingTests
{
    [Fact]
    public void TrainBooking_Properties_SetCorrectly()
    {
        // Arrange
        var train = new Train { Id = 1, TrainNumber = "12951" };
        var bookedAt = DateTime.UtcNow;

        // Act
        var booking = new TrainBooking
        {
            Id = 1001,
            PNR = 8234567890,
            UserId = 42,
            TrainId = 1,
            Train = train,
            TravelDate = new DateOnly(2026, 6, 15),
            SeatClass = "2AC",
            PassengerDetails = "[{\"name\":\"John Doe\",\"age\":35,\"gender\":\"Male\"}]",
            TotalAmount = 3000.00m,
            Status = "Confirmed",
            BookedAt = bookedAt
        };

        // Assert
        Assert.Equal(1001, booking.Id);
        Assert.Equal(8234567890, booking.PNR);
        Assert.Equal(42, booking.UserId);
        Assert.Equal(1, booking.TrainId);
        Assert.NotNull(booking.Train);
        Assert.Equal(new DateOnly(2026, 6, 15), booking.TravelDate);
        Assert.Equal("2AC", booking.SeatClass);
        Assert.Equal("[{\"name\":\"John Doe\",\"age\":35,\"gender\":\"Male\"}]", booking.PassengerDetails);
        Assert.Equal(3000.00m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
        Assert.Equal(bookedAt, booking.BookedAt);
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    public void TrainBooking_ValidStatuses(string status)
    {
        // Arrange & Act
        var booking = new TrainBooking { Status = status };

        // Assert
        Assert.Equal(status, booking.Status);
    }

    [Theory]
    [InlineData("Sleeper")]
    [InlineData("3AC")]
    [InlineData("2AC")]
    [InlineData("1AC")]
    public void TrainBooking_ValidSeatClasses(string seatClass)
    {
        // Arrange & Act
        var booking = new TrainBooking { SeatClass = seatClass };

        // Assert
        Assert.Equal(seatClass, booking.SeatClass);
    }
}
