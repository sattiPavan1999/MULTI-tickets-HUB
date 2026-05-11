using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class TrainServiceClient(HttpClient httpClient, ILogger<TrainServiceClient> logger) : ITrainService
{
    private static async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        ErrorResponse? body = null;
        try { body = await response.Content.ReadFromJsonAsync<ErrorResponse>(ct); } catch { }
        var message = body?.Message ?? response.ReasonPhrase ?? "Upstream request failed";
        throw new ProxyException((int)response.StatusCode, message);
    }

    public async Task<List<TrainDto>> GetAllTrainsAsync(CancellationToken ct = default)
    {
        var trains = await httpClient.GetFromJsonAsync<List<TrainDto>>("api/trains", ct);
        return trains ?? [];
    }

    public async Task<TrainDto> CreateTrainAsync(CreateTrainRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/trains", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<TrainDto>(ct))!;
    }

    public async Task<TrainDto> UpdateTrainAsync(int id, UpdateTrainRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{id}", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<TrainDto>(ct))!;
    }

    public async Task DeleteTrainAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/trains/{id}", ct);
        await ThrowIfErrorAsync(response, ct);
    }

    public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int trainId, CancellationToken ct = default)
    {
        var seats = await httpClient.GetFromJsonAsync<List<SeatAvailabilityDto>>($"api/trains/{trainId}/seat-availability", ct);
        return seats ?? [];
    }

    public async Task<SeatAvailabilityDto> UpdateSeatAvailabilityAsync(int trainId, UpdateSeatAvailabilityRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{trainId}/seat-availability", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<SeatAvailabilityDto>(ct))!;
    }
}
