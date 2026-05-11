using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IShowtimeRepository showtimeRepository,
    IValidator<CreateBookingInput> validator,
    IMapper mapper,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateBookingAsync(CreateBookingInput input, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(input, ct);

        var showtime = await showtimeRepository.GetByIdAsync(input.ShowtimeId, ct)
            ?? throw new NotFoundException($"Showtime {input.ShowtimeId} not found");

        var existingBookings = await bookingRepository.GetByShowtimeAsync(input.ShowtimeId, ct);

        var alreadyBooked = existingBookings
            .Where(b => !string.IsNullOrWhiteSpace(b.SeatNumbers))
            .SelectMany(b => b.SeatNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .ToHashSet();

        var conflicting = input.SeatNumbers.Where(alreadyBooked.Contains).ToList();
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
            Status = "Pending",
            BookedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        showtime.AvailableSeats -= input.SeatNumbers.Count;
        await showtimeRepository.UpdateAsync(showtime, ct);

        var created = await bookingRepository.AddAsync(booking, ct);
        logger.LogInformation("Booking created for showtime {ShowtimeId} by user {UserId}, seats {Seats}",
            input.ShowtimeId, input.UserId, booking.SeatNumbers);

        return mapper.Map<BookingResponse>(created);
    }
}
