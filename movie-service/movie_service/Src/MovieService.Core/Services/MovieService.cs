using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
    public async Task<List<MovieResponse>> GetAllMoviesAsync(bool? activeOnly = null)
    {
        var query = movieRepository.Query();
        if (activeOnly is true)
            query = query.Where(m => m.IsActive);
        var movies = await query.ToListAsync();
        return mapper.Map<List<MovieResponse>>(movies);
    }

    public async Task<MovieResponse?> GetMovieByIdAsync(int id)
    {
        var movie = await movieRepository.GetByIdAsync(id);
        return movie is null ? null : mapper.Map<MovieResponse>(movie);
    }

    public async Task<MovieResponse> CreateMovieAsync(CreateMovieInput input)
    {
        await createValidator.ValidateAndThrowAsync(input);

        var movie = new Movie
        {
            Title = input.Title,
            Genre = input.Genre,
            Duration = input.Duration,
            PosterUrl = input.PosterUrl,
            IsActive = true
        };

        var created = await movieRepository.AddAsync(movie);
        logger.LogInformation("Movie created: {Title} (Id={Id})", created.Title, created.Id);
        return mapper.Map<MovieResponse>(created);
    }

    public async Task<MovieResponse> UpdateMovieAsync(int id, UpdateMovieInput input)
    {
        await updateValidator.ValidateAndThrowAsync(input);

        var movie = await movieRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Movie {id} not found");

        if (input.Title is not null) movie.Title = input.Title;
        if (input.Genre is not null) movie.Genre = input.Genre;
        if (input.Duration.HasValue) movie.Duration = input.Duration.Value;
        if (input.PosterUrl is not null) movie.PosterUrl = input.PosterUrl;

        var updated = await movieRepository.UpdateAsync(movie);
        logger.LogInformation("Movie updated: Id={Id}", id);
        return mapper.Map<MovieResponse>(updated);
    }

    public async Task DeleteMovieAsync(int id)
    {
        var movie = await movieRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Movie {id} not found");

        await movieRepository.DeleteAsync(movie.Id);
        logger.LogInformation("Movie deleted: Id={Id}", id);
    }

    public async Task<OperationResult> ToggleMovieStatusAsync(int id)
    {
        var movie = await movieRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Movie {id} not found");

        movie.IsActive = !movie.IsActive;
        await movieRepository.UpdateAsync(movie);

        var status = movie.IsActive ? "activated" : "deactivated";
        logger.LogInformation("Movie {Id} {Status}", id, status);
        return new OperationResult { Success = true, Message = $"Movie {status}" };
    }
}
