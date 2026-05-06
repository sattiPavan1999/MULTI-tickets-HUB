using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieService.Data;
using MovieService.Models;
using MovieService.Repositories;
using MovieService.Services;

namespace MovieService.Tests.Services;

public class BookingServiceTests
{
    private class MockLogger : ILogger<BookingServiceImpl>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task BookSeatsAsync_Should_ThrowException_WhenNoSeatsSelected()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bookingRepo = new BookingRepository(context);
        var showRepo = new ShowRepository(context);
        var seatRepo = new SeatRepository(context);
        var service = new BookingServiceImpl(bookingRepo, showRepo, seatRepo, context, new MockLogger());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.BookSeatsAsync(1, 1, Array.Empty<int>()));
    }

    [Fact]
    public async Task BookSeatsAsync_Should_ThrowException_WhenMoreThan10Seats()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bookingRepo = new BookingRepository(context);
        var showRepo = new ShowRepository(context);
        var seatRepo = new SeatRepository(context);
        var service = new BookingServiceImpl(bookingRepo, showRepo, seatRepo, context, new MockLogger());

        var seats = Enumerable.Range(1, 11).ToArray();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.BookSeatsAsync(1, 1, seats));
    }

    [Fact]
    public async Task BookSeatsAsync_Should_ThrowException_WhenShowNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bookingRepo = new BookingRepository(context);
        var showRepo = new ShowRepository(context);
        var seatRepo = new SeatRepository(context);
        var service = new BookingServiceImpl(bookingRepo, showRepo, seatRepo, context, new MockLogger());

        // Act & Assert
        await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() =>
            service.BookSeatsAsync(1, 999, new[] { 1, 2 }));
    }

    [Fact]
    public async Task CancelBookingAsync_Should_ThrowException_WhenBookingNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var bookingRepo = new BookingRepository(context);
        var showRepo = new ShowRepository(context);
        var seatRepo = new SeatRepository(context);
        var service = new BookingServiceImpl(bookingRepo, showRepo, seatRepo, context, new MockLogger());

        // Act & Assert
        await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() =>
            service.CancelBookingAsync(999, 1));
    }
}
