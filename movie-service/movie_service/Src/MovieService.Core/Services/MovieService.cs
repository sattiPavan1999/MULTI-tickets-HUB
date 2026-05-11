using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class MovieService(
    IMovieRepository movieRepository,
    IValidator<CreateMovieInput> createValidator,
    IValidator<UpdateMovieInput> updateValidator,
    IMapper mapper,
    ILogger<MovieService> logger) : IMovieService
{
    public async Task<List<MovieResponse>> GetAllMoviesAsync(bool? activeOnly = null, CancellationToken ct = default)
    {
        var movies = await movieRepository.GetAllAsync(ct);
        if (activeOnly is true)
            movies = movies.Where(m => m.IsActive).ToList();
        return mapper.Map<List<MovieResponse>>(movies);
    }

    public async Task<MovieResponse?> GetMovieByIdAsync(int id, CancellationToken ct = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, ct);
        return movie is null ? null : mapper.Map<MovieResponse>(movie);
    }

    public async Task<MovieResponse> CreateMovieAsync(CreateMovieInput input, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(input, ct);

        var movie = new Movie
        {
            Title = input.Title,
            Genre = input.Genre,
            Duration = input.Duration,
            PosterUrl = input.PosterUrl,
            IsActive = true
        };

        var created = await movieRepository.AddAsync(movie, ct);
        logger.LogInformation("Movie created: {Title} (Id={Id})", created.Title, created.Id);
        return mapper.Map<MovieResponse>(created);
    }

    public async Task<MovieResponse> UpdateMovieAsync(int id, UpdateMovieInput input, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(input, ct);

        var movie = await movieRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Movie {id} not found");

        if (input.Title is not null) movie.Title = input.Title;
        if (input.Genre is not null) movie.Genre = input.Genre;
        if (input.Duration.HasValue) movie.Duration = input.Duration.Value;
        if (input.PosterUrl is not null) movie.PosterUrl = input.PosterUrl;

        var updated = await movieRepository.UpdateAsync(movie, ct);
        logger.LogInformation("Movie updated: Id={Id}", id);
        return mapper.Map<MovieResponse>(updated);
    }

    public async Task DeleteMovieAsync(int id, CancellationToken ct = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Movie {id} not found");

        await movieRepository.DeleteAsync(movie.Id, ct);
        logger.LogInformation("Movie deleted: Id={Id}", id);
    }

    public async Task<OperationResult> ToggleMovieStatusAsync(int id, CancellationToken ct = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Movie {id} not found");

        movie.IsActive = !movie.IsActive;
        await movieRepository.UpdateAsync(movie, ct);

        var status = movie.IsActive ? "activated" : "deactivated";
        logger.LogInformation("Movie {Id} {Status}", id, status);
        return new OperationResult { Success = true, Message = $"Movie {status}" };
    }
}
