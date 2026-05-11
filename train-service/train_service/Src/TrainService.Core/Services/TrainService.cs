using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TrainService.Core.DTOs;
using TrainService.Core.Exceptions;
using TrainService.Core.Models;
using TrainService.Core.Repositories;

namespace TrainService.Core.Services;

public class TrainService(
    ITrainRepository trainRepository,
    ISeatAvailabilityRepository seatRepository,
    IValidator<CreateTrainInput> createValidator,
    IValidator<UpdateTrainInput> updateValidator,
    IValidator<SeatAvailabilityInput> seatValidator,
    IMapper mapper,
    ILogger<TrainService> logger) : ITrainService
{
    public async Task<List<TrainResponse>> GetAllTrainsAsync(CancellationToken ct = default)
    {
        var trains = await trainRepository.GetAllAsync(ct);
        return mapper.Map<List<TrainResponse>>(trains);
    }

    public async Task<List<TrainResponse>> SearchTrainsAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false, CancellationToken ct = default)
    {
        var trains = await trainRepository.SearchByRouteAsync(source, destination, sortBy, requiresAvailability, ct);
        return mapper.Map<List<TrainResponse>>(trains);
    }

    public async Task<TrainResponse?> GetTrainByIdAsync(int id, CancellationToken ct = default)
    {
        var train = await trainRepository.GetByIdAsync(id, ct);
        return train is null ? null : mapper.Map<TrainResponse>(train);
    }

    public async Task<TrainResponse> CreateTrainAsync(CreateTrainInput input, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(input, ct);

        if (await trainRepository.GetByTrainNumberAsync(input.TrainNumber, ct) is not null)
            throw new ConflictException($"Train number '{input.TrainNumber}' already exists");

        var train = new Train
        {
            TrainName = input.TrainName,
            TrainNumber = input.TrainNumber,
            Source = input.Source,
            Destination = input.Destination,
            DepartureTime = DateTime.SpecifyKind(input.DepartureTime, DateTimeKind.Utc),
            ArrivalTime = DateTime.SpecifyKind(input.ArrivalTime, DateTimeKind.Utc),
            Price = input.Price
        };

        var created = await trainRepository.AddAsync(train, ct);
        logger.LogInformation("Train created: {TrainNumber} (Id={Id})", created.TrainNumber, created.Id);
        return mapper.Map<TrainResponse>(created);
    }

    public async Task<TrainResponse> UpdateTrainAsync(int id, UpdateTrainInput input, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(input, ct);

        var train = await trainRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Train {id} not found");

        if (input.TrainName is not null) train.TrainName = input.TrainName;
        if (input.Source is not null) train.Source = input.Source;
        if (input.Destination is not null) train.Destination = input.Destination;
        if (input.DepartureTime.HasValue) train.DepartureTime = DateTime.SpecifyKind(input.DepartureTime.Value, DateTimeKind.Utc);
        if (input.ArrivalTime.HasValue) train.ArrivalTime = DateTime.SpecifyKind(input.ArrivalTime.Value, DateTimeKind.Utc);
        if (input.Price.HasValue) train.Price = input.Price.Value;

        if (input.TrainNumber is not null && input.TrainNumber != train.TrainNumber)
        {
            if (await trainRepository.GetByTrainNumberAsync(input.TrainNumber, ct) is not null)
                throw new ConflictException($"Train number '{input.TrainNumber}' already exists");
            train.TrainNumber = input.TrainNumber;
        }

        var updated = await trainRepository.UpdateAsync(train, ct);
        return mapper.Map<TrainResponse>(updated);
    }

    public async Task DeleteTrainAsync(int id, CancellationToken ct = default)
    {
        var train = await trainRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Train {id} not found");

        await trainRepository.DeleteAsync(train.Id, ct);
        logger.LogInformation("Train deleted: Id={Id}", id);
    }

    public async Task<List<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(int trainId, CancellationToken ct = default)
    {
        _ = await trainRepository.GetByIdAsync(trainId, ct)
            ?? throw new NotFoundException($"Train {trainId} not found");

        var seats = await seatRepository.GetByTrainAsync(trainId, ct);
        return mapper.Map<List<SeatAvailabilityResponse>>(seats);
    }

    public async Task<SeatAvailabilityResponse> UpdateSeatAvailabilityAsync(int trainId, SeatAvailabilityInput input, CancellationToken ct = default)
    {
        await seatValidator.ValidateAndThrowAsync(input, ct);

        _ = await trainRepository.GetByIdAsync(trainId, ct)
            ?? throw new NotFoundException($"Train {trainId} not found");

        var availability = new SeatAvailability
        {
            TrainId = trainId,
            Date = input.Date,
            AvailableSeats = input.AvailableSeats
        };

        var result = await seatRepository.UpsertAsync(availability, ct);
        return mapper.Map<SeatAvailabilityResponse>(result);
    }
}
