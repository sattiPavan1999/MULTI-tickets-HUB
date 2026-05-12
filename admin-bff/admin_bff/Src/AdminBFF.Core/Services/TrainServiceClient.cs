using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;

namespace AdminBFF.Core.Services;

public class TrainServiceClient(HttpClient httpClient) : ITrainService
{
    private static async Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        ErrorResponse? body = null;
        try { body = await response.Content.ReadFromJsonAsync<ErrorResponse>(); } catch { }
        var message = body?.Message ?? response.ReasonPhrase ?? "Upstream request failed";
        throw new ProxyException((int)response.StatusCode, message);
    }

    public async Task<List<TrainDto>> GetAllTrainsAsync()
    {
        var trains = await httpClient.GetFromJsonAsync<List<TrainDto>>("api/trains");
        return trains ?? [];
    }

    public async Task<TrainDto> CreateTrainAsync(CreateTrainRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/trains", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<TrainDto>())!;
    }

    public async Task<TrainDto> UpdateTrainAsync(int id, UpdateTrainRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{id}", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<TrainDto>())!;
    }

    public async Task DeleteTrainAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/trains/{id}");
        await ThrowIfErrorAsync(response);
    }

    public async Task<List<SeatAvailabilityDto>> GetSeatAvailabilityAsync(int trainId)
    {
        var seats = await httpClient.GetFromJsonAsync<List<SeatAvailabilityDto>>($"api/trains/{trainId}/seat-availability");
        return seats ?? [];
    }

    public async Task<SeatAvailabilityDto> UpdateSeatAvailabilityAsync(int trainId, UpdateSeatAvailabilityRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/trains/{trainId}/seat-availability", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<SeatAvailabilityDto>())!;
    }
}
