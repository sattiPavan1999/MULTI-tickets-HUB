using TrainService.DTOs;

namespace TrainService.Tests.DTOs;

public class BookingResponseTests
{
    [Fact]
    public void BookingResponse_Properties_SetCorrectly()
    {
        // Arrange
        var passengers = new List<PassengerDetail>
        {
            new PassengerDetail { Name = "John Doe", Age = 35, Gender = "Male" }
        };
        var bookedAt = DateTime.UtcNow;

        // Act
        var response = new BookingResponse
        {
            Id = 1001,
            Pnr = 8234567890,
            UserId = 42,
            TrainId = 1,
            TravelDate = new DateOnly(2026, 6, 15),
            SeatClass = "2AC",
            PassengerDetails = passengers,
            TotalAmount = 3000.00m,
            Status = "Confirmed",
            BookedAt = bookedAt
        };

        // Assert
        Assert.Equal(1001, response.Id);
        Assert.Equal(8234567890, response.Pnr);
        Assert.Equal(42, response.UserId);
        Assert.Equal(1, response.TrainId);
        Assert.Equal(new DateOnly(2026, 6, 15), response.TravelDate);
        Assert.Equal("2AC", response.SeatClass);
        Assert.Single(response.PassengerDetails);
        Assert.Equal(3000.00m, response.TotalAmount);
        Assert.Equal("Confirmed", response.Status);
        Assert.Equal(bookedAt, response.BookedAt);
    }

    [Fact]
    public void CancelBookingResponse_Properties_SetCorrectly()
    {
        // Arrange & Act
        var response = new CancelBookingResponse
        {
            Id = 1001,
            Pnr = 8234567890,
            Status = "Cancelled"
        };

        // Assert
        Assert.Equal(1001, response.Id);
        Assert.Equal(8234567890, response.Pnr);
        Assert.Equal("Cancelled", response.Status);
    }
}
