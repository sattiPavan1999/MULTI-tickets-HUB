using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class TrainServiceClient(HttpClient httpClient, ILogger<TrainServiceClient> logger) : ITrainService
{
    public async Task<List<TrainDto>> GetAllTrainsAsync(CancellationToken ct = default)
    {
        var trains = await httpClient.GetFromJsonAsync<List<TrainDto>>("api/trains", ct);
        return trains ?? [];
    }

    public async Task<TrainDto> CreateTrainAsync(CreateTrainRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/trains", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TrainDto>(ct))!;
    }

    public async Task<TrainDto> UpdateTrainAsync(int id, UpdateTrainRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{id}", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TrainDto>(ct))!;
    }

    public async Task DeleteTrainAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/trains/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int trainId, CancellationToken ct = default)
    {
        var seats = await httpClient.GetFromJsonAsync<List<SeatAvailabilityDto>>($"api/trains/{trainId}/seat-availability", ct);
        return seats ?? [];
    }

    public async Task<SeatAvailabilityDto> UpdateSeatAvailabilityAsync(int trainId, UpdateSeatAvailabilityRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{trainId}/seat-availability", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SeatAvailabilityDto>(ct))!;
    }
}
