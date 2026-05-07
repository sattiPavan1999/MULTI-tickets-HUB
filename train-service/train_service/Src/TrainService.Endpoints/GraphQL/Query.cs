using TrainService.Core.DTOs;
using TrainService.Core.Services;

namespace TrainService.Endpoints.GraphQL;

public class Query
{
    public async Task<List<TrainResponse>> SearchTrains(
        SearchTrainInput input,
        [Service] ITrainService trainService)
    {
        return await trainService.SearchTrainsAsync(input);
    }

    public async Task<TrainResponse> GetTrainById(
        int trainId,
        DateOnly travelDate,
        [Service] ITrainService trainService)
    {
        return await trainService.GetTrainByIdAsync(trainId, travelDate);
    }

    public async Task<BookingResponse> GetBooking(
        int bookingId,
        int userId,
        [Service] IBookingService bookingService)
    {
        return await bookingService.GetBookingAsync(bookingId, userId);
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
