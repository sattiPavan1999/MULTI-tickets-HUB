using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TrainService.Core.Data;
using TrainService.Core.Models;

namespace TrainService.Core.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly TrainDbContext _context;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(TrainDbContext context, ILogger<BookingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TrainBooking> CreateBookingAsync(TrainBooking booking)
    {
        _logger.LogInformation("Creating booking for user {UserId} on train {TrainId}", booking.UserId, booking.TrainId);

        _context.TrainBookings.Add(booking);
        await _context.SaveChangesAsync();

        return booking;
    }

    public async Task<TrainBooking?> GetBookingByIdAsync(int bookingId)
    {
        _logger.LogInformation("Fetching booking with ID: {BookingId}", bookingId);

        return await _context.TrainBookings
            .Include(b => b.Train)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<List<TrainBooking>> GetUserBookingsAsync(int userId)
    {
        _logger.LogInformation("Fetching bookings for user {UserId}", userId);

        return await _context.TrainBookings
            .Include(b => b.Train)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<int> GetBookedSeatsCountAsync(int trainId, DateOnly travelDate, string seatClass)
    {
        _logger.LogInformation("Calculating booked seats for train {TrainId} on {TravelDate} for class {SeatClass}",
            trainId, travelDate, seatClass);

        var bookings = await _context.TrainBookings
            .Where(b => b.TrainId == trainId
                && b.TravelDate == travelDate
                && b.SeatClass == seatClass
                && b.Status == "Confirmed")
            .AsNoTracking()
            .ToListAsync();

        int totalBooked = 0;
        foreach (var booking in bookings)
        {
            var passengers = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(booking.PassengerDetails);
            totalBooked += passengers?.Count ?? 0;
        }

        return totalBooked;
    }

    public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
    {
        _logger.LogInformation("Updating booking {BookingId} status to {Status}", bookingId, status);

        var booking = await _context.TrainBookings.FindAsync(bookingId);
        if (booking == null)
        {
            return false;
        }

        booking.Status = status;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TrainBooking>> GetAllAsync()
    {
        return await _context.TrainBookings
            .AsNoTracking()
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
    }

    public async Task<long> GenerateUniquePNRAsync()
    {
        var random = new Random();
        long pnr;
        bool exists;

        do
        {
            pnr = (long)(random.NextDouble() * 9000000000) + 1000000000;
            exists = await _context.TrainBookings.AnyAsync(b => b.PNR == pnr);
        }
        while (exists);

        _logger.LogInformation("Generated unique PNR: {PNR}", pnr);
        return pnr;
    }
}
