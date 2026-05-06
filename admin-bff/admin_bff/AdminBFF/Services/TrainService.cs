using System.Net.Http.Json;
using AdminBFF.DTOs;
using AdminBFF.Models;

namespace AdminBFF.Services;

public class TrainService : ITrainService
{
    private readonly HttpClient _httpClient;
    private readonly GraphQLHttpClient _graphql;
    private readonly ILogger<TrainService> _logger;

    public TrainService(HttpClient httpClient, ILogger<TrainService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _graphql = new GraphQLHttpClient(httpClient, logger);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        const string query = @"
            query {
                allBookings { id userId pnr totalAmount status bookedAt }
            }";
        var bookings = await _graphql.QueryAsync<List<TrainBookingPayload>>(query, null, "allBookings");
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            BookingType = "Train",
            Pnr = (int)b.Pnr,
            ShowId = null,
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
                throw new ServiceUnavailableException("Train Service returned error", new Exception($"Status: {response.StatusCode}"));
            }

            return new OperationResultDto { Success = true, Message = "Booking cancelled successfully" };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Train Service");
            throw new ServiceUnavailableException("Train Service unavailable", ex);
        }
    }

    public async Task<TrainDto> AddTrainAsync(AddTrainInput input)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/trains", input);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new ValidationException(await response.Content.ReadAsStringAsync());
                }
                throw new ServiceUnavailableException("Train Service returned error", new Exception($"Status: {response.StatusCode}"));
            }

            var train = await response.Content.ReadFromJsonAsync<TrainDto>();
            return train ?? throw new ServiceUnavailableException("Invalid response from Train Service", new Exception("Null train returned"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Train Service");
            throw new ServiceUnavailableException("Train Service unavailable", ex);
        }
    }

    private record TrainBookingPayload
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public long Pnr { get; init; }
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
