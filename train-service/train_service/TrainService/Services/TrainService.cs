using System.Text.Json;
using TrainService.DTOs;
using TrainService.Repositories;

namespace TrainService.Services;

public class TrainService : ITrainService
{
    private readonly ITrainRepository _trainRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<TrainService> _logger;

    public TrainService(
        ITrainRepository trainRepository,
        IBookingRepository bookingRepository,
        IAuditService auditService,
        ILogger<TrainService> logger)
    {
        _trainRepository = trainRepository;
        _bookingRepository = bookingRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<List<TrainResponse>> SearchTrainsAsync(SearchTrainInput input)
    {
        if (input.SourceStation == input.DestinationStation)
        {
            throw new ArgumentException("Source and destination stations must be different");
        }

        if (input.TravelDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Travel date must be today or a future date");
        }

        var trains = await _trainRepository.SearchTrainsAsync(input.SourceStation, input.DestinationStation);
        var responses = new List<TrainResponse>();

        foreach (var train in trains)
        {
            var availability = await CalculateAvailabilityAsync(train.Id, input.TravelDate, train.TotalSeats);
            var fares = ParseFares(train.Fares);

            responses.Add(new TrainResponse
            {
                Id = train.Id,
                TrainNumber = train.TrainNumber,
                TrainName = train.TrainName,
                SourceStation = train.SourceStation,
                DestinationStation = train.DestinationStation,
                DepartureTime = train.DepartureTime.ToString(@"hh\:mm\:ss"),
                ArrivalTime = train.ArrivalTime.ToString(@"hh\:mm\:ss"),
                AvailableSeats = availability,
                Fare = fares
            });
        }

        _auditService.LogSearch(input.SourceStation, input.DestinationStation, input.TravelDate, responses.Count);

        return responses;
    }

    public async Task<TrainResponse> GetTrainByIdAsync(int trainId, DateOnly travelDate)
    {
        if (travelDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Travel date must be today or a future date");
        }

        var train = await _trainRepository.GetTrainByIdAsync(trainId);
        if (train == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Train with ID {trainId} not found");
        }

        var availability = await CalculateAvailabilityAsync(train.Id, travelDate, train.TotalSeats);
        var fares = ParseFares(train.Fares);

        return new TrainResponse
        {
            Id = train.Id,
            TrainNumber = train.TrainNumber,
            TrainName = train.TrainName,
            SourceStation = train.SourceStation,
            DestinationStation = train.DestinationStation,
            DepartureTime = train.DepartureTime.ToString(@"hh\:mm\:ss"),
            ArrivalTime = train.ArrivalTime.ToString(@"hh\:mm\:ss"),
            AvailableSeats = availability,
            Fare = fares
        };
    }

    private async Task<SeatAvailabilityDto> CalculateAvailabilityAsync(int trainId, DateOnly travelDate, string totalSeatsJson)
    {
        var totalSeats = JsonSerializer.Deserialize<Dictionary<string, int>>(totalSeatsJson);
        if (totalSeats == null)
        {
            throw new InvalidOperationException("Invalid total seats data");
        }

        var bookedSleeper = await _bookingRepository.GetBookedSeatsCountAsync(trainId, travelDate, "Sleeper");
        var bookedAC3 = await _bookingRepository.GetBookedSeatsCountAsync(trainId, travelDate, "3AC");
        var bookedAC2 = await _bookingRepository.GetBookedSeatsCountAsync(trainId, travelDate, "2AC");
        var bookedAC1 = await _bookingRepository.GetBookedSeatsCountAsync(trainId, travelDate, "1AC");

        return new SeatAvailabilityDto
        {
            Sleeper = Math.Max(0, totalSeats.GetValueOrDefault("sleeper", 0) - bookedSleeper),
            Ac3Tier = Math.Max(0, totalSeats.GetValueOrDefault("ac3Tier", 0) - bookedAC3),
            Ac2Tier = Math.Max(0, totalSeats.GetValueOrDefault("ac2Tier", 0) - bookedAC2),
            Ac1Tier = Math.Max(0, totalSeats.GetValueOrDefault("ac1Tier", 0) - bookedAC1)
        };
    }

    private FareDto ParseFares(string faresJson)
    {
        var fares = JsonSerializer.Deserialize<Dictionary<string, decimal>>(faresJson);
        if (fares == null)
        {
            throw new InvalidOperationException("Invalid fares data");
        }

        return new FareDto
        {
            Sleeper = fares.GetValueOrDefault("sleeper", 0),
            Ac3Tier = fares.GetValueOrDefault("ac3Tier", 0),
            Ac2Tier = fares.GetValueOrDefault("ac2Tier", 0),
            Ac1Tier = fares.GetValueOrDefault("ac1Tier", 0)
        };
    }
}
