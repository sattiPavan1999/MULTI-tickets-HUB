using System.Net.Http.Json;
using AdminBFF.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace AdminBFF.Core.Services;

public class MovieServiceClient(HttpClient httpClient, ILogger<MovieServiceClient> logger) : IMovieService
{
    public async Task<List<MovieDto>> GetAllMoviesAsync(CancellationToken ct = default)
    {
        var movies = await httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies", ct);
        return movies ?? [];
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/movies", request, ct);
        response.EnsureSuccessStatusCode();
        var movie = await response.Content.ReadFromJsonAsync<MovieDto>(ct);
        return movie!;
    }

    public async Task<MovieDto> UpdateMovieAsync(int id, UpdateMovieRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/movies/{id}", request, ct);
        response.EnsureSuccessStatusCode();
        var movie = await response.Content.ReadFromJsonAsync<MovieDto>(ct);
        return movie!;
    }

    public async Task DeleteMovieAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/movies/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<OperationResult> ToggleMovieStatusAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsync($"api/movies/{id}/toggle-status", null, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OperationResult>(ct);
        return result ?? new OperationResult { Success = true };
    }
}
