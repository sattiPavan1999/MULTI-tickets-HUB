using System.Data;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieService.Core.Data;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Models;

namespace MovieService.Core.Services;

public class BookingService(
    IValidator<CreateBookingInput> validator,
    IMapper mapper,
    MovieDbContext dbContext,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingInput input, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(input, ct);

        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        try
        {
            var showtime = await dbContext.Showtimes.FindAsync([input.ShowtimeId], ct)
                ?? throw new NotFoundException($"Showtime {input.ShowtimeId} not found");

            var alreadyBooked = await dbContext.Bookings
                .Where(b => b.ShowtimeId == input.ShowtimeId && !string.IsNullOrEmpty(b.SeatNumbers))
                .Select(b => b.SeatNumbers)
                .ToListAsync(ct);

            var bookedSet = alreadyBooked
                .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(int.Parse)
                .ToHashSet();

            var conflicting = input.SeatNumbers.Where(bookedSet.Contains).ToList();
            if (conflicting.Count > 0)
                throw new ConflictException($"Seats {string.Join(", ", conflicting)} are already booked");

            if (input.SeatNumbers.Count > showtime.AvailableSeats)
                throw new ConflictException("Not enough available seats");

            var booking = new MovieBooking
            {
                ShowtimeId = input.ShowtimeId,
                UserId = input.UserId,
                SeatNumbers = string.Join(",", input.SeatNumbers),
                NumberOfSeats = input.SeatNumbers.Count,
                Status = "Confirmed",
                BookedAt = DateTime.UtcNow
            };

            showtime.AvailableSeats -= input.SeatNumbers.Count;
            dbContext.Showtimes.Update(showtime);
            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            logger.LogInformation("Booking created for showtime {ShowtimeId} by user {UserId}, seats {Seats}",
                input.ShowtimeId, input.UserId, booking.SeatNumbers);

            return mapper.Map<BookingResponse>(booking);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            throw new ConflictException("Seats filled — another booking completed first. Please try again.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
