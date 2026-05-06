using MovieService.DTOs;
using MovieService.GraphQL.Types;
using MovieService.Services;

namespace MovieService.GraphQL.Queries;

public class Query
{
    public async Task<List<MovieType>> GetMovies(
        [Service] IMovieService movieService,
        string? genre = null,
        string? language = null,
        string? format = null)
    {
        var movies = await movieService.GetMoviesAsync(genre, language, format);
        return movies.Select(MovieType.FromDto).ToList();
    }

    public async Task<List<ShowType>> GetShowsByMovie(
        [Service] IShowService showService,
        int movieId,
        DateTime? date = null)
    {
        var shows = await showService.GetShowsByMovieAsync(movieId, date);
        return shows.Select(ShowType.FromDto).ToList();
    }

    public async Task<List<SeatType>> GetSeatMap(
        [Service] ISeatService seatService,
        int showId)
    {
        var seats = await seatService.GetSeatMapAsync(showId);
        return seats.Select(SeatType.FromDto).ToList();
    }

    public async Task<BookingType> GetBooking(
        [Service] IBookingService bookingService,
        int bookingId,
        int userId)
    {
        var booking = await bookingService.GetBookingAsync(bookingId, userId);
        return BookingType.FromDto(booking);
    }

    public async Task<List<AdminBookingDto>> GetAllBookings([Service] IBookingService bookingService)
    {
        return await bookingService.GetAllBookingsAsync();
    }

    public async Task<BookingStatsDto> GetBookingStats([Service] IBookingService bookingService)
    {
        return await bookingService.GetBookingStatsAsync();
    }
}
