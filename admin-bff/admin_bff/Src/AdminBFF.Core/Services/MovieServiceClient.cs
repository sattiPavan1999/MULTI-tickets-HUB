using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class MovieServiceClient(HttpClient httpClient, ILogger<MovieServiceClient> logger) : IMovieService
{
    private static async Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        ErrorResponse? body = null;
        try { body = await response.Content.ReadFromJsonAsync<ErrorResponse>(); } catch { }
        var message = body?.Message ?? response.ReasonPhrase ?? "Upstream request failed";
        throw new ProxyException((int)response.StatusCode, message);
    }

    public async Task<List<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies");
        return movies ?? [];
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/movies", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
    }

    public async Task<MovieDto> UpdateMovieAsync(int id, UpdateMovieRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"api/movies/{id}", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<MovieDto>())!;
    }

    public async Task DeleteMovieAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/movies/{id}");
        await ThrowIfErrorAsync(response);
    }

    public async Task<OperationResult> ToggleMovieStatusAsync(int id)
    {
        var response = await httpClient.PutAsync($"api/movies/{id}/toggle-status", null);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<OperationResult>())
               ?? new OperationResult { Success = true };
    }

    public async Task<List<ShowtimeDto>> GetShowtimesAsync(int movieId)
    {
        var showtimes = await httpClient.GetFromJsonAsync<List<ShowtimeDto>>($"api/movies/{movieId}/showtimes");
        return showtimes ?? [];
    }

    public async Task<ShowtimeDto> CreateShowtimeAsync(CreateShowtimeRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"api/movies/{request.MovieId}/showtimes", request);
        await ThrowIfErrorAsync(response);
        return (await response.Content.ReadFromJsonAsync<ShowtimeDto>())!;
    }

    public async Task DeleteShowtimeAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"api/movies/showtimes/{id}");
        await ThrowIfErrorAsync(response);
    }
}
