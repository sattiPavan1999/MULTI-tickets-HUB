using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Models;
using MovieService.Core.Repositories;

namespace MovieService.Core.Services;

public class ShowtimeService(
    IShowtimeRepository showtimeRepository,
    IMovieRepository movieRepository,
    IBookingRepository bookingRepository,
    IValidator<CreateShowtimeInput> validator,
    IMapper mapper,
    ILogger<ShowtimeService> logger) : IShowtimeService
{
    public async Task<List<ShowtimeResponse>> GetShowtimesByMovieAsync(int movieId, CancellationToken ct = default)
    {
        var showtimes = await showtimeRepository.GetByMovieIdAsync(movieId, ct);
        return mapper.Map<List<ShowtimeResponse>>(showtimes);
    }

    public async Task<ShowtimeResponse> CreateShowtimeAsync(CreateShowtimeInput input, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(input, ct);

        var showDate = DateOnly.Parse(input.ShowDate);
        var showTime = TimeOnly.Parse(input.ShowTime);

        var movie = await movieRepository.GetByIdAsync(input.MovieId, ct)
            ?? throw new NotFoundException($"Movie {input.MovieId} not found");

        // Check for exact duplicate on the same movie
        var exactDuplicate = await showtimeRepository.GetByCompositeKeyAsync(
            input.MovieId, showDate, showTime, input.ScreenNumber, ct);
        if (exactDuplicate is not null)
            throw new ConflictException($"This exact showtime already exists for {movie.Title}");

        // Enforce 4-hour gap rule: no two showtimes within 4 hours on the same screen on the same date
        var screenShowtimes = await showtimeRepository.GetByScreenAndDateAsync(input.ScreenNumber, showDate, ct);
        var conflict = screenShowtimes.FirstOrDefault(s =>
            Math.Abs((showTime - s.ShowTime).TotalHours) < 4);
        if (conflict is not null)
        {
            var gap = Math.Abs((showTime - conflict.ShowTime).TotalHours);
            throw new ConflictException(
                $"Screen {input.ScreenNumber} already has a showtime at {conflict.ShowTime:HH\\:mm} on {input.ShowDate}. " +
                $"A minimum 4-hour gap is required between showtimes on the same screen (current gap: {gap:F1}h).");
        }

        var showtime = new Showtime
        {
            MovieId = input.MovieId,
            ShowDate = showDate,
            ShowTime = showTime,
            ScreenNumber = input.ScreenNumber,
            TotalSeats = input.TotalSeats,
            AvailableSeats = input.TotalSeats
        };

        var created = await showtimeRepository.AddAsync(showtime, ct);
        logger.LogInformation("Showtime created for movie {MovieId} on {Date} at {Time}", input.MovieId, input.ShowDate, input.ShowTime);
        return mapper.Map<ShowtimeResponse>(created);
    }

    public async Task DeleteShowtimeAsync(int id, CancellationToken ct = default)
    {
        var showtime = await showtimeRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Showtime {id} not found");

        await showtimeRepository.DeleteAsync(showtime.Id, ct);
        logger.LogInformation("Showtime {Id} deleted", id);
    }

    public async Task<SeatStatusResponse> GetSeatStatusAsync(int showtimeId, CancellationToken ct = default)
    {
        var showtime = await showtimeRepository.GetByIdAsync(showtimeId, ct)
            ?? throw new NotFoundException($"Showtime {showtimeId} not found");

        var bookings = await bookingRepository.GetByShowtimeAsync(showtimeId, ct);

        var bookedSeats = bookings
            .Where(b => !string.IsNullOrWhiteSpace(b.SeatNumbers))
            .SelectMany(b => b.SeatNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(int.Parse)
            .ToList();

        return new SeatStatusResponse
        {
            ShowtimeId = showtimeId,
            TotalSeats = showtime.TotalSeats,
            BookedSeats = bookedSeats
        };
    }
}
