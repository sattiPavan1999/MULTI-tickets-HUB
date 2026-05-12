using System.Data;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainService.Core.Data;
using TrainService.Core.DTOs;
using TrainService.Core.Exceptions;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Core.Services;

public class TrainBookingService(
    ITrainBookingRepository bookingRepository,
    ISeatAvailabilityRepository seatRepository,
    IValidator<CreateTrainBookingInput> validator,
    IMapper mapper,
    TrainDbContext dbContext,
    ILogger<TrainBookingService> logger) : ITrainBookingService
{
    private static readonly TimeZoneInfo Ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
    public async Task<TrainBookingResponse> CreateBookingAsync(CreateTrainBookingInput input)
    {
        await validator.ValidateAndThrowAsync(input);

        var travelDate = DateOnly.ParseExact(input.TravelDate, "yyyy-MM-dd");

        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        try
        {
            var train = await dbContext.Trains.FindAsync([input.TrainId])
                ?? throw new NotFoundException($"Train {input.TrainId} not found.");

            if (DateTime.UtcNow >= train.DepartureTime.AddHours(-1))
                throw new ConflictException("Booking is closed. Train departs within 1 hour or has already departed.");

            var seat = await seatRepository.GetByTrainAndDateAsync(input.TrainId, travelDate)
                ?? throw new NotFoundException($"No seat availability found for train {input.TrainId} on {input.TravelDate}. Please select a date configured by the admin.");

            var pnr = "PNR" + Guid.NewGuid().ToString("N").ToUpper()[..8];

            TrainBooking booking;

            if (seat.AvailableSeats >= input.NumberOfSeats)
            {
                seat.AvailableSeats -= input.NumberOfSeats;
                dbContext.SeatAvailabilities.Update(seat);

                booking = new TrainBooking
                {
                    TrainId = input.TrainId,
                    UserId = input.UserId,
                    TravelDate = travelDate,
                    PassengerName = input.PassengerName,
                    PassengerAge = input.PassengerAge,
                    NumberOfSeats = input.NumberOfSeats,
                    PNR = pnr,
                    Status = BookingStatus.Confirmed,
                    WaitlistPosition = null,
                    BookedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist)
                };
            }
            else if (seat.AvailableSeats == 0)
            {
                var waitlistCount = await dbContext.Bookings
                    .CountAsync(b => b.TrainId == input.TrainId && b.TravelDate == travelDate && b.Status == BookingStatus.Waitlisted);

                booking = new TrainBooking
                {
                    TrainId = input.TrainId,
                    UserId = input.UserId,
                    TravelDate = travelDate,
                    PassengerName = input.PassengerName,
                    PassengerAge = input.PassengerAge,
                    NumberOfSeats = input.NumberOfSeats,
                    PNR = pnr,
                    Status = BookingStatus.Waitlisted,
                    WaitlistPosition = waitlistCount + 1,
                    BookedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Ist)
                };
            }
            else
            {
                throw new ConflictException($"Only {seat.AvailableSeats} seat(s) available. Please reduce your seat count.");
            }

            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync();
            await tx.CommitAsync();

            logger.LogInformation("Booking created: PNR={PNR}, Status={Status}, TrainId={TrainId}", booking.PNR, booking.Status, booking.TrainId);
            return mapper.Map<TrainBookingResponse>(booking);
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

    public async Task<List<TrainBookingResponse>> GetMyBookingsAsync(int userId)
    {
        var bookings = await bookingRepository.GetByUserIdAsync(userId);
        return mapper.Map<List<TrainBookingResponse>>(bookings);
    }

    public async Task<TrainBookingResponse> GetBookingByIdAsync(int bookingId, int userId)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new NotFoundException($"Booking {bookingId} not found");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this booking");

        return mapper.Map<TrainBookingResponse>(booking);
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

            if (DateTime.UtcNow >= booking.Train.DepartureTime.AddHours(-2))
                throw new ConflictException("Cancellation is not allowed within 2 hours of departure");

            var wasConfirmed = booking.Status == BookingStatus.Confirmed;
            booking.Status = BookingStatus.Cancelled;
            booking.WaitlistPosition = null;
            dbContext.Bookings.Update(booking);

            if (wasConfirmed)
            {
                var seat = await seatRepository.GetByTrainAndDateAsync(booking.TrainId, booking.TravelDate);
                if (seat is not null)
                {
                    seat.AvailableSeats += booking.NumberOfSeats;
                    dbContext.SeatAvailabilities.Update(seat);
                }
                // PromoteWaitlistAsync flushes all pending changes (cancellation + seat free + promotion)
                // in a single SaveChangesAsync call
                await PromoteWaitlistAsync(booking.TrainId, booking.TravelDate);
            }
            else
            {
                var remaining = await bookingRepository.GetWaitlistedByTrainAndDateAsync(booking.TrainId, booking.TravelDate);
                for (var i = 0; i < remaining.Count; i++)
                    remaining[i].WaitlistPosition = i + 1;
                dbContext.Bookings.UpdateRange(remaining);
                await dbContext.SaveChangesAsync();
            }

            await tx.CommitAsync();

            logger.LogInformation("Booking {BookingId} cancelled (PNR={PNR}) by user {UserId}", bookingId, booking.PNR, userId);
            return new OperationResult { Success = true, Message = "Booking cancelled successfully" };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task PromoteWaitlistAsync(int trainId, DateOnly date)
    {
        var waitlisted = await bookingRepository.GetWaitlistedByTrainAndDateAsync(trainId, date);
        if (waitlisted.Count == 0) return;

        var first = waitlisted[0];

        var seat = await seatRepository.GetByTrainAndDateAsync(trainId, date);
        if (seat is not null && seat.AvailableSeats >= first.NumberOfSeats)
        {
            seat.AvailableSeats -= first.NumberOfSeats;
            dbContext.SeatAvailabilities.Update(seat);
        }

        first.Status = BookingStatus.Confirmed;
        first.WaitlistPosition = null;
        dbContext.Bookings.Update(first);

        for (var i = 1; i < waitlisted.Count; i++)
            waitlisted[i].WaitlistPosition = i;
        if (waitlisted.Count > 1)
            dbContext.Bookings.UpdateRange(waitlisted.Skip(1));

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Promoted PNR={PNR} from Waitlisted to Confirmed for TrainId={TrainId} on {Date}", first.PNR, trainId, date);
    }
}
