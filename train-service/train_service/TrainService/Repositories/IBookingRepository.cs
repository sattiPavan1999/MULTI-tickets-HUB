using TrainService.Models;

namespace TrainService.Repositories;

public interface IBookingRepository
{
    Task<TrainBooking> CreateBookingAsync(TrainBooking booking);
    Task<TrainBooking?> GetBookingByIdAsync(int bookingId);
    Task<List<TrainBooking>> GetUserBookingsAsync(int userId);
    Task<int> GetBookedSeatsCountAsync(int trainId, DateOnly travelDate, string seatClass);
    Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
    Task<long> GenerateUniquePNRAsync();

    Task<List<TrainBooking>> GetAllAsync();
}
