using AdminBFF.Core.DTOs;

namespace AdminBFF.Tests.DTOs;

public class BookingDtoTests
{
    [Fact]
    public void BookingDto_Should_Initialize_Train_Booking()
    {
        // Arrange & Act
        var booking = new BookingDto
        {
            Id = 101,
            UserId = 5,
            BookingType = "Train",
            Pnr = 123456789,
            ShowId = null,
            TotalAmount = 1500.00m,
            Status = "Confirmed",
            BookedAt = new DateTime(2026, 5, 10, 14, 30, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal(101, booking.Id);
        Assert.Equal(5, booking.UserId);
        Assert.Equal("Train", booking.BookingType);
        Assert.Equal(123456789, booking.Pnr);
        Assert.Null(booking.ShowId);
        Assert.Equal(1500.00m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
    }

    [Fact]
    public void BookingDto_Should_Initialize_Movie_Booking()
    {
        // Arrange & Act
        var booking = new BookingDto
        {
            Id = 202,
            UserId = 7,
            BookingType = "Movie",
            Pnr = null,
            ShowId = 45,
            TotalAmount = 800.00m,
            Status = "Confirmed",
            BookedAt = new DateTime(2026, 5, 11, 18, 0, 0, DateTimeKind.Utc)
        };

        // Assert
        Assert.Equal(202, booking.Id);
        Assert.Equal(7, booking.UserId);
        Assert.Equal("Movie", booking.BookingType);
        Assert.Null(booking.Pnr);
        Assert.Equal(45, booking.ShowId);
        Assert.Equal(800.00m, booking.TotalAmount);
        Assert.Equal("Confirmed", booking.Status);
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    public void BookingDto_Should_Accept_Valid_Status(string status)
    {
        // Act
        var booking = new BookingDto
        {
            Id = 1,
            UserId = 1,
            BookingType = "Train",
            Pnr = 123456,
            ShowId = null,
            TotalAmount = 100.00m,
            Status = status,
            BookedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(status, booking.Status);
    }

    [Theory]
    [InlineData("Train")]
    [InlineData("Movie")]
    public void BookingDto_Should_Accept_Valid_BookingType(string bookingType)
    {
        // Act
        var booking = new BookingDto
        {
            Id = 1,
            UserId = 1,
            BookingType = bookingType,
            Pnr = bookingType == "Train" ? 123456 : null,
            ShowId = bookingType == "Movie" ? 45 : null,
            TotalAmount = 100.00m,
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(bookingType, booking.BookingType);
    }

    [Fact]
    public void BookingDto_Should_Be_Immutable()
    {
        // Arrange
        var original = new BookingDto
        {
            Id = 1,
            UserId = 1,
            BookingType = "Train",
            Pnr = 123456,
            ShowId = null,
            TotalAmount = 100.00m,
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow
        };

        // Act
        var modified = original with { Status = "Cancelled" };

        // Assert
        Assert.Equal("Confirmed", original.Status);
        Assert.Equal("Cancelled", modified.Status);
    }
}
