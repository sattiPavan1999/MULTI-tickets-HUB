using System.Net.Http.Json;
using AdminBFF.DTOs;
using AdminBFF.Models;

namespace AdminBFF.Services;

public class MovieService : IMovieService
{
    private readonly HttpClient _httpClient;
    private readonly GraphQLHttpClient _graphql;
    private readonly ILogger<MovieService> _logger;

    public MovieService(HttpClient httpClient, ILogger<MovieService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _graphql = new GraphQLHttpClient(httpClient, logger);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        const string query = @"
            query {
                allBookings { id userId showId totalAmount status bookedAt }
            }";
        var bookings = await _graphql.QueryAsync<List<MovieBookingPayload>>(query, null, "allBookings");
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            BookingType = "Movie",
            Pnr = null,
            ShowId = b.ShowId,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            BookedAt = b.BookedAt
        }).ToList();
    }

    public async Task<Dictionary<string, int>> GetBookingStatsAsync()
    {
        const string query = "query { bookingStats { total cancelled } }";
        var stats = await _graphql.QueryAsync<StatsPayload>(query, null, "bookingStats");
        return new Dictionary<string, int> { ["total"] = stats.Total, ["cancelled"] = stats.Cancelled };
    }

    public async Task<OperationResultDto> CancelBookingAsync(int bookingId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/bookings/{bookingId}/cancel", null);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new NotFoundException($"Booking with ID {bookingId} not found");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ValidationException(await response.Content.ReadAsStringAsync());
                }
                throw new ServiceUnavailableException("Movie Service returned error", new Exception($"Status: {response.StatusCode}"));
            }

            return new OperationResultDto { Success = true, Message = "Booking cancelled successfully" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Movie Service");
            throw new ServiceUnavailableException("Movie Service unavailable", ex);
        }
    }

    public async Task<MovieDto> AddMovieAsync(AddMovieInput input)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/movies", input);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ValidationException(await response.Content.ReadAsStringAsync());
                }
                throw new ServiceUnavailableException("Movie Service returned error", new Exception($"Status: {response.StatusCode}"));
            }

            var movie = await response.Content.ReadFromJsonAsync<MovieDto>();
            return movie ?? throw new ServiceUnavailableException("Invalid response from Movie Service", new Exception("Null movie returned"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Movie Service");
            throw new ServiceUnavailableException("Movie Service unavailable", ex);
        }
    }

    private record MovieBookingPayload
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int ShowId { get; init; }
        public decimal TotalAmount { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime BookedAt { get; init; }
    }

    private record StatsPayload
    {
        public int Total { get; init; }
        public int Cancelled { get; init; }
    }
}
