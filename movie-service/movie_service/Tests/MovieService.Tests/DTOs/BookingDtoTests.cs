using MovieService.Core.DTOs;

namespace MovieService.Tests.DTOs;

public class BookingDtoTests
{
    [Fact]
    public void BookingDto_Should_SetAndGetProperties()
    {
        // Arrange
        var bookedAt = DateTime.UtcNow;
        var selectedSeats = new[] { 1, 2, 3 };

        // Act
        var dto = new BookingDto
        {
            Id = 1,
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = selectedSeats,
            TotalAmount = 600.00m,
            Status = "Confirmed",
            BookedAt = bookedAt,
            CancelledAt = null
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal(100, dto.UserId);
        Assert.Equal(50, dto.ShowId);
        Assert.Equal(selectedSeats, dto.SelectedSeatIds);
        Assert.Equal(600.00m, dto.TotalAmount);
        Assert.Equal("Confirmed", dto.Status);
        Assert.Equal(bookedAt, dto.BookedAt);
        Assert.Null(dto.CancelledAt);
    }

    [Fact]
    public void BookingDto_Should_AllowCancelledStatus()
    {
        // Arrange
        var cancelledAt = DateTime.UtcNow;

        // Act
        var dto = new BookingDto
        {
            Id = 1,
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = new[] { 1, 2 },
            TotalAmount = 400.00m,
            Status = "Cancelled",
            BookedAt = DateTime.UtcNow.AddHours(-2),
            CancelledAt = cancelledAt
        };

        // Assert
        Assert.Equal("Cancelled", dto.Status);
        Assert.NotNull(dto.CancelledAt);
        Assert.Equal(cancelledAt, dto.CancelledAt);
    }

    [Fact]
    public void BookingDto_Should_AllowNestedShowAndSeats()
    {
        // Arrange & Act
        var dto = new BookingDto
        {
            Id = 1,
            UserId = 100,
            ShowId = 50,
            SelectedSeatIds = new[] { 1, 2 },
            TotalAmount = 400.00m,
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow,
            Show = new ShowDto
            {
                Id = 50,
                MovieId = 1,
                ScreenId = 5,
                ShowTime = DateTime.UtcNow,
                AvailableSeats = 100
            },
            Seats = new List<SeatDto>
            {
                new() { Id = 1, ScreenId = 5, RowLabel = "A", SeatNumber = 1, Category = "Regular", Price = 200m, IsAvailable = false },
                new() { Id = 2, ScreenId = 5, RowLabel = "A", SeatNumber = 2, Category = "Regular", Price = 200m, IsAvailable = false }
            }
        };

        // Assert
        Assert.NotNull(dto.Show);
        Assert.NotNull(dto.Seats);
        Assert.Equal(2, dto.Seats.Count);
    }
}
