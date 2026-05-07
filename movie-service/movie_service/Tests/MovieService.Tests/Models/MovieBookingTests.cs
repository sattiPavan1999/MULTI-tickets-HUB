using MovieService.Core.Models;

namespace MovieService.Tests.Models;

public class MovieBookingTests
{
    [Fact]
    public void MovieBooking_Should_SetAndGetProperties()
    {
        // Arrange
        var bookedAt = DateTime.UtcNow;
        var selectedSeats = new[] { 1, 2, 3 };

        // Act
        var booking = new MovieBooking
        {
            Id = 1,
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = selectedSeats,
            TotalAmount = 600.00m,
            Status = "Confirmed",
            BookedAt = bookedAt
        };

        // Assert
        Assert.Equal(1, booking.Id);
        Assert.Equal(100, booking.UserId);
        Assert.Equal(50, booking.ShowId);
        Assert.Equal(selectedSeats, booking.SelectedSeatIds);
        Assert.Equal(600.00m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
        Assert.Equal(bookedAt, booking.BookedAt);
        Assert.Null(booking.CancelledAt);
    }

    [Fact]
    public void MovieBooking_Should_AllowMultipleSeats()
    {
        // Arrange
        var seats = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        // Act
        var booking = new MovieBooking
        {
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = seats,
            TotalAmount = 2000m,
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(10, booking.SelectedSeatIds.Length);
        Assert.Equal(seats, booking.SelectedSeatIds);
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    public void MovieBooking_Should_AllowValidStatuses(string status)
    {
        // Arrange & Act
        var booking = new MovieBooking
        {
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = new[] { 1, 2 },
            TotalAmount = 400m,
            Status = status,
            BookedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(status, booking.Status);
    }

    [Fact]
    public void MovieBooking_Should_SetCancelledAtWhenCancelled()
    {
        // Arrange
        var cancelledAt = DateTime.UtcNow;

        // Act
        var booking = new MovieBooking
        {
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = new[] { 1, 2 },
            TotalAmount = 400m,
            Status = "Cancelled",
            BookedAt = DateTime.UtcNow.AddHours(-2),
            CancelledAt = cancelledAt
        };

        // Assert
        Assert.Equal("Cancelled", booking.Status);
        Assert.NotNull(booking.CancelledAt);
        Assert.Equal(cancelledAt, booking.CancelledAt);
    }
}
