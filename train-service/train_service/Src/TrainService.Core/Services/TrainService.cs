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
    public async Task<List<TrainResponse>> GetAllTrainsAsync()
    {
        var trains = await trainRepository.GetAllAsync();
        return mapper.Map<List<TrainResponse>>(trains);
    }

    public async Task<List<TrainResponse>> SearchTrainsAsync(string? source, string? destination, string? sortBy, bool requiresAvailability = false)
    {
        var trains = await trainRepository.SearchByRouteAsync(source, destination, sortBy, requiresAvailability);
        return mapper.Map<List<TrainResponse>>(trains);
    }

    public async Task<TrainResponse?> GetTrainByIdAsync(int id)
    {
        var train = await trainRepository.GetByIdAsync(id);
        return train is null ? null : mapper.Map<TrainResponse>(train);
    }

    public async Task<TrainResponse> CreateTrainAsync(CreateTrainInput input)
    {
        await createValidator.ValidateAndThrowAsync(input);

        if (await trainRepository.GetByTrainNumberAsync(input.TrainNumber) is not null)
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

        var created = await trainRepository.AddAsync(train);
        logger.LogInformation("Train created: {TrainNumber} (Id={Id})", created.TrainNumber, created.Id);
        return mapper.Map<TrainResponse>(created);
    }

    public async Task<TrainResponse> UpdateTrainAsync(int id, UpdateTrainInput input)
    {
        await updateValidator.ValidateAndThrowAsync(input);

        var train = await trainRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Train {id} not found");

        if (input.TrainName is not null) train.TrainName = input.TrainName;
        if (input.Source is not null) train.Source = input.Source;
        if (input.Destination is not null) train.Destination = input.Destination;
        if (input.DepartureTime.HasValue) train.DepartureTime = DateTime.SpecifyKind(input.DepartureTime.Value, DateTimeKind.Utc);
        if (input.ArrivalTime.HasValue) train.ArrivalTime = DateTime.SpecifyKind(input.ArrivalTime.Value, DateTimeKind.Utc);
        if (input.Price.HasValue) train.Price = input.Price.Value;

        if (input.TrainNumber is not null && input.TrainNumber != train.TrainNumber)
        {
            if (await trainRepository.GetByTrainNumberAsync(input.TrainNumber) is not null)
                throw new ConflictException($"Train number '{input.TrainNumber}' already exists");
            train.TrainNumber = input.TrainNumber;
        }

        var updated = await trainRepository.UpdateAsync(train);
        return mapper.Map<TrainResponse>(updated);
    }

    public async Task DeleteTrainAsync(int id)
    {
        var train = await trainRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Train {id} not found");

        await trainRepository.DeleteAsync(train.Id);
        logger.LogInformation("Train deleted: Id={Id}", id);
    }

    public async Task<List<SeatAvailabilityResponse>> GetSeatAvailabilityAsync(int trainId)
    {
        _ = await trainRepository.GetByIdAsync(trainId)
            ?? throw new NotFoundException($"Train {trainId} not found");

        var seats = await seatRepository.GetByTrainAsync(trainId);
        return mapper.Map<List<SeatAvailabilityResponse>>(seats);
    }

    public async Task<SeatAvailabilityResponse> UpdateSeatAvailabilityAsync(int trainId, SeatAvailabilityInput input)
    {
        await seatValidator.ValidateAndThrowAsync(input);

        _ = await trainRepository.GetByIdAsync(trainId)
            ?? throw new NotFoundException($"Train {trainId} not found");

        var availability = new SeatAvailability
        {
            TrainId = trainId,
            Date = input.Date,
            AvailableSeats = input.AvailableSeats
        };

        var result = await seatRepository.UpsertAsync(availability);
        return mapper.Map<SeatAvailabilityResponse>(result);
    }
}
