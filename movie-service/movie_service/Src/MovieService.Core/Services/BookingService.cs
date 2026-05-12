using System.Data;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieService.Core.Data;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class BookingService(
    IValidator<CreateBookingInput> validator,
    IMapper mapper,
    MovieDbContext dbContext,
    IBookingRepository bookingRepository,
    ILogger<BookingService> logger) : IBookingService
{
    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingInput input)
    {
        await validator.ValidateAndThrowAsync(input);

        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        try
        {
            var showtime = await dbContext.Showtimes.FindAsync([input.ShowtimeId])
                ?? throw new NotFoundException($"Showtime {input.ShowtimeId} not found");

            if (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist) >= showtime.ShowDate.ToDateTime(showtime.ShowTime))
                throw new ConflictException("Booking is closed — this show has already started");

            var alreadyBooked = await dbContext.Bookings
                .Where(b => b.ShowtimeId == input.ShowtimeId && !string.IsNullOrEmpty(b.SeatNumbers))
                .Select(b => b.SeatNumbers)
                .ToListAsync();

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
                Status = BookingStatus.Confirmed,
                BookedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist)
            };

            showtime.AvailableSeats -= input.SeatNumbers.Count;
            dbContext.Showtimes.Update(showtime);
            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync();
            await tx.CommitAsync();

            logger.LogInformation("Booking created for showtime {ShowtimeId} by user {UserId}, seats {Seats}",
                input.ShowtimeId, input.UserId, booking.SeatNumbers);

            return mapper.Map<BookingResponse>(booking);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Seats filled — another booking completed first. Please try again.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(int userId)
    {
        var bookings = await bookingRepository.GetByUserIdAsync(userId);
        return mapper.Map<List<BookingResponse>>(bookings);
    }

    public async Task<BookingResponse> GetBookingByIdAsync(int bookingId, int userId)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this booking");

        return mapper.Map<BookingResponse>(booking);
    }

    public async Task<OperationResult> CancelBookingAsync(int bookingId, int userId)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        try
        {
            var booking = await bookingRepository.GetByIdWithDetailsAsync(bookingId)
                ?? throw new NotFoundException($"Booking {bookingId} not found");

            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking");

            if (booking.Status == BookingStatus.Cancelled)
                throw new ConflictException("Booking is already cancelled");

            var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist);
            if (istNow >= booking.Showtime.ShowDate.ToDateTime(booking.Showtime.ShowTime).AddHours(-2))
                throw new ConflictException("Cancellation is not allowed within 2 hours of the show");

            booking.Status = BookingStatus.Cancelled;
            booking.Showtime.AvailableSeats += booking.NumberOfSeats;
            dbContext.Bookings.Update(booking);
            dbContext.Showtimes.Update(booking.Showtime);
            await dbContext.SaveChangesAsync();
            await tx.CommitAsync();

            logger.LogInformation("Booking {BookingId} cancelled by user {UserId}", bookingId, userId);
            return new OperationResult { Success = true, Message = "Booking cancelled successfully" };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
