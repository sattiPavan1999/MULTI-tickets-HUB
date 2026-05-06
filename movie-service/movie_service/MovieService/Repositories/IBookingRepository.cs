using MovieService.Models;

namespace MovieService.Repositories;

public interface IBookingRepository
{
    Task<MovieBooking> CreateBookingAsync(MovieBooking booking);
    Task<MovieBooking?> GetBookingByIdAsync(int id);
    Task<MovieBooking?> GetBookingWithDetailsAsync(int id);
    Task UpdateBookingAsync(MovieBooking booking);

    Task<List<MovieBooking>> GetAllAsync();
}
