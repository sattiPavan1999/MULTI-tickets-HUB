using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class MovieServiceClient(HttpClient httpClient, ILogger<MovieServiceClient> logger) : IMovieService
{
    private static async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        ErrorResponse? body = null;
        try { body = await response.Content.ReadFromJsonAsync<ErrorResponse>(ct); } catch { }
        var message = body?.Message ?? response.ReasonPhrase ?? "Upstream request failed";
        throw new ProxyException((int)response.StatusCode, message);
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync(CancellationToken ct = default)
    {
        var movies = await httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies", ct);
        return movies ?? [];
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/movies", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<MovieDto>(ct))!;
    }

    public async Task<MovieDto> UpdateMovieAsync(int id, UpdateMovieRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/movies/{id}", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<MovieDto>(ct))!;
    }

    public async Task DeleteMovieAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/movies/{id}", ct);
        await ThrowIfErrorAsync(response, ct);
    }

    public async Task<OperationResult> ToggleMovieStatusAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsync($"api/movies/{id}/toggle-status", null, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<OperationResult>(ct))
               ?? new OperationResult { Success = true };
    }

    public async Task<List<ShowtimeDto>> GetShowtimesAsync(int movieId, CancellationToken ct = default)
    {
        var showtimes = await httpClient.GetFromJsonAsync<List<ShowtimeDto>>($"api/movies/{movieId}/showtimes", ct);
        return showtimes ?? [];
    }

    public async Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/movies/{request.MovieId}/showtimes", request, ct);
        await ThrowIfErrorAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<ShowtimeDto>(ct))!;
    }

    public async Task DeleteShowtimeAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/movies/showtimes/{id}", ct);
        await ThrowIfErrorAsync(response, ct);
    }
}
