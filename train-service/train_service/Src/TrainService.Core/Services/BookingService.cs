using Microsoft.Extensions.Logging;
using System.Text.Json;
using TrainService.Core.DTOs;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Core.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITrainRepository _trainRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IBookingRepository bookingRepository,
        ITrainRepository trainRepository,
        IAuditService auditService,
        ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _trainRepository = trainRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingInput input)
    {
        ValidateBookingInput(input);

        var train = await _trainRepository.GetTrainByIdAsync(input.TrainId);
        if (train == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Train with ID {input.TrainId} not found");
        }

        if (input.TravelDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Travel date must be today or a future date");
        }

        var totalSeats = JsonSerializer.Deserialize<Dictionary<string, int>>(train.TotalSeats);
        if (totalSeats == null)
        {
            throw new InvalidOperationException("Invalid train seat configuration");
        }

        var bookedSeats = await _bookingRepository.GetBookedSeatsCountAsync(
            input.TrainId, input.TravelDate, input.SeatClass);

        var seatKey = GetSeatKey(input.SeatClass);
        var availableSeats = totalSeats.GetValueOrDefault(seatKey, 0) - bookedSeats;

        if (availableSeats < input.NumberOfPassengers)
        {
            throw new InvalidOperationException(
                $"Insufficient seats available. Only {availableSeats} seats available in {input.SeatClass}");
        }

        var fares = JsonSerializer.Deserialize<Dictionary<string, decimal>>(train.Fares);
        if (fares == null)
        {
            throw new InvalidOperationException("Invalid train fare configuration");
        }

        var farePerSeat = fares.GetValueOrDefault(seatKey, 0);
        var totalAmount = farePerSeat * input.NumberOfPassengers;

        var pnr = await _bookingRepository.GenerateUniquePNRAsync();

        var booking = new TrainBooking
        {
            PNR = pnr,
            UserId = input.UserId,
            TrainId = input.TrainId,
            TravelDate = input.TravelDate,
            SeatClass = input.SeatClass,
            PassengerDetails = JsonSerializer.Serialize(input.PassengerDetails),
            TotalAmount = totalAmount,
            Status = "Confirmed",
            BookedAt = DateTime.UtcNow
        };

        var createdBooking = await _bookingRepository.CreateBookingAsync(booking);

        _auditService.LogBookingCreation(
            pnr, input.UserId, input.TrainId, input.SeatClass, input.NumberOfPassengers, totalAmount);

        return new BookingResponse
        {
            Id = createdBooking.Id,
            Pnr = createdBooking.PNR,
            UserId = createdBooking.UserId,
            TrainId = createdBooking.TrainId,
            TravelDate = createdBooking.TravelDate,
            SeatClass = createdBooking.SeatClass,
            PassengerDetails = input.PassengerDetails,
            TotalAmount = createdBooking.TotalAmount,
            Status = createdBooking.Status,
            BookedAt = createdBooking.BookedAt
        };
    }

    public async Task<BookingResponse> GetBookingAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Booking with ID {bookingId} not found");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this booking");
        }

        var passengerDetails = JsonSerializer.Deserialize<List<PassengerDetail>>(booking.PassengerDetails);

        return new BookingResponse
        {
            Id = booking.Id,
            Pnr = booking.PNR,
            UserId = booking.UserId,
            TrainId = booking.TrainId,
            TravelDate = booking.TravelDate,
            SeatClass = booking.SeatClass,
            PassengerDetails = passengerDetails ?? new List<PassengerDetail>(),
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            BookedAt = booking.BookedAt
        };
    }

    public async Task<CancelBookingResponse> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Booking with ID {bookingId} not found");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to cancel this booking");
        }

        if (booking.Status != "Confirmed")
        {
            throw new InvalidOperationException("Booking is already cancelled");
        }

        var success = await _bookingRepository.UpdateBookingStatusAsync(bookingId, "Cancelled");
        if (!success)
        {
            throw new InvalidOperationException("Failed to cancel booking");
        }

        _auditService.LogBookingCancellation(booking.PNR, userId, bookingId);

        return new CancelBookingResponse
        {
            Id = bookingId,
            Pnr = booking.PNR,
            Status = "Cancelled"
        };
    }

    public async Task<List<AdminBookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return bookings.Select(b => new AdminBookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            Pnr = b.PNR,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            BookedAt = b.BookedAt
        }).ToList();
    }

    public async Task<BookingStatsDto> GetBookingStatsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return new BookingStatsDto
        {
            Total = bookings.Count,
            Cancelled = bookings.Count(b => b.Status == "Cancelled")
        };
    }

    private void ValidateBookingInput(CreateBookingInput input)
    {
        if (input.NumberOfPassengers != input.PassengerDetails.Count)
        {
            throw new ArgumentException("Number of passengers must match passenger details count");
        }

        var validSeatClasses = new[] { "Sleeper", "3AC", "2AC", "1AC" };
        if (!validSeatClasses.Contains(input.SeatClass))
        {
            throw new ArgumentException($"Invalid seat class. Must be one of: {string.Join(", ", validSeatClasses)}");
        }

        foreach (var passenger in input.PassengerDetails)
        {
            if (string.IsNullOrWhiteSpace(passenger.Name))
            {
                throw new ArgumentException("Passenger name must not be empty");
            }

            if (passenger.Age <= 0)
            {
                throw new ArgumentException("Passenger age must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(passenger.Gender))
            {
                throw new ArgumentException("Passenger gender is required");
            }
        }
    }

    private string GetSeatKey(string seatClass)
    {
        return seatClass switch
        {
            "Sleeper" => "sleeper",
            "3AC" => "ac3Tier",
            "2AC" => "ac2Tier",
            "1AC" => "ac1Tier",
            _ => throw new ArgumentException($"Invalid seat class: {seatClass}")
        };
    }
}
