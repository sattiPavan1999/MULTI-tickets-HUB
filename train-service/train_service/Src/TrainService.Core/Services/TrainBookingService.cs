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
    public async Task<TrainBookingResponse> CreateBookingAsync(CreateTrainBookingInput input, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(input, ct);

        var travelDate = DateOnly.ParseExact(input.TravelDate, "yyyy-MM-dd");

        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        try
        {
            var train = await dbContext.Trains.FindAsync([input.TrainId], ct)
                ?? throw new NotFoundException($"Train {input.TrainId} not found.");

            // Booking closes 1 hour before departure
            if (DateTime.UtcNow >= train.DepartureTime.AddHours(-1))
                throw new ConflictException("Booking is closed. Train departs within 1 hour or has already departed.");

            var seat = await seatRepository.GetByTrainAndDateAsync(input.TrainId, travelDate, ct)
                ?? throw new NotFoundException($"No seat availability found for train {input.TrainId} on {input.TravelDate}. Please select a date configured by the admin.");

            var pnr = "PNR" + Guid.NewGuid().ToString("N").ToUpper()[..8];

            TrainBooking booking;

            if (seat.AvailableSeats >= input.NumberOfSeats)
            {
                seat.AvailableSeats -= input.NumberOfSeats;
                await seatRepository.UpsertAsync(seat, ct);

                booking = new TrainBooking
                {
                    TrainId = input.TrainId,
                    UserId = input.UserId,
                    TravelDate = travelDate,
                    PassengerName = input.PassengerName,
                    PassengerAge = input.PassengerAge,
                    NumberOfSeats = input.NumberOfSeats,
                    PNR = pnr,
                    Status = "Confirmed",
                    WaitlistPosition = null,
                    BookedAt = DateTime.UtcNow
                };
            }
            else if (seat.AvailableSeats == 0)
            {
                var waitlistCount = await dbContext.Bookings
                    .CountAsync(b => b.TrainId == input.TrainId && b.TravelDate == travelDate && b.Status == "Waitlisted", ct);

                booking = new TrainBooking
                {
                    TrainId = input.TrainId,
                    UserId = input.UserId,
                    TravelDate = travelDate,
                    PassengerName = input.PassengerName,
                    PassengerAge = input.PassengerAge,
                    NumberOfSeats = input.NumberOfSeats,
                    PNR = pnr,
                    Status = "Waitlisted",
                    WaitlistPosition = waitlistCount + 1,
                    BookedAt = DateTime.UtcNow
                };
            }
            else
            {
                throw new ConflictException($"Only {seat.AvailableSeats} seat(s) available. Please reduce your seat count.");
            }

            var created = await bookingRepository.AddAsync(booking, ct);
            await tx.CommitAsync(ct);

            logger.LogInformation("Booking created: PNR={PNR}, Status={Status}, TrainId={TrainId}", created.PNR, created.Status, created.TrainId);
            return mapper.Map<TrainBookingResponse>(created);
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

    public async Task PromoteWaitlistAsync(int trainId, DateOnly date, CancellationToken ct = default)
    {
        var waitlisted = await bookingRepository.GetWaitlistedByTrainAndDateAsync(trainId, date, ct);
        if (waitlisted.Count == 0) return;

        var first = waitlisted[0];

        // Consume the freed seat for the promoted booking
        var seat = await seatRepository.GetByTrainAndDateAsync(trainId, date, ct);
        if (seat is not null && seat.AvailableSeats >= first.NumberOfSeats)
        {
            seat.AvailableSeats -= first.NumberOfSeats;
            await seatRepository.UpsertAsync(seat, ct);
        }

        first.Status = "Confirmed";
        first.WaitlistPosition = null;
        await bookingRepository.UpdateAsync(first, ct);

        for (var i = 1; i < waitlisted.Count; i++)
        {
            waitlisted[i].WaitlistPosition = i;
            await bookingRepository.UpdateAsync(waitlisted[i], ct);
        }

        logger.LogInformation("Promoted PNR={PNR} from Waitlisted to Confirmed for TrainId={TrainId} on {Date}", first.PNR, trainId, date);
    }

    public async Task<OperationResult> CancelBookingAsync(int bookingId, CancellationToken ct = default)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        try
        {
            var booking = await bookingRepository.GetByIdAsync(bookingId, ct)
                ?? throw new NotFoundException($"Booking {bookingId} not found");

            if (booking.Status == "Cancelled")
                throw new ConflictException("Booking is already cancelled");

            var wasConfirmed = booking.Status == "Confirmed";
            booking.Status = "Cancelled";
            booking.WaitlistPosition = null;
            await bookingRepository.UpdateAsync(booking, ct);

            if (wasConfirmed)
            {
                var seat = await seatRepository.GetByTrainAndDateAsync(booking.TrainId, booking.TravelDate, ct);
                if (seat is not null)
                {
                    seat.AvailableSeats += booking.NumberOfSeats;
                    await seatRepository.UpsertAsync(seat, ct);
                }
            }
            else
            {
                // Renumber remaining waitlist positions
                var remaining = await bookingRepository.GetWaitlistedByTrainAndDateAsync(booking.TrainId, booking.TravelDate, ct);
                for (var i = 0; i < remaining.Count; i++)
                {
                    remaining[i].WaitlistPosition = i + 1;
                    await bookingRepository.UpdateAsync(remaining[i], ct);
                }
            }

            await tx.CommitAsync(ct);

            if (wasConfirmed)
                await PromoteWaitlistAsync(booking.TrainId, booking.TravelDate, ct);

            logger.LogInformation("Booking {BookingId} cancelled (PNR={PNR})", bookingId, booking.PNR);
            return new OperationResult { Success = true, Message = "Booking cancelled successfully" };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
