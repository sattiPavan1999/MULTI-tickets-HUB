using Microsoft.EntityFrameworkCore;
using MovieService.Data;
using MovieService.DTOs;
using MovieService.Models;
using MovieService.Repositories;

namespace MovieService.Services;

public class BookingServiceImpl : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IShowRepository _showRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<BookingServiceImpl> _logger;

    public BookingServiceImpl(
        IBookingRepository bookingRepository,
        IShowRepository showRepository,
        ISeatRepository seatRepository,
        AppDbContext context,
        ILogger<BookingServiceImpl> logger)
    {
        _bookingRepository = bookingRepository;
        _showRepository = showRepository;
        _seatRepository = seatRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<BookingDto> BookSeatsAsync(int userId, int showId, int[] selectedSeatIds)
    {
        if (selectedSeatIds == null || selectedSeatIds.Length == 0)
        {
            throw new ArgumentException("At least one seat must be selected");
        }

        if (selectedSeatIds.Length > 10)
        {
            throw new ArgumentException("Maximum 10 seats allowed per booking");
        }

        var show = await _showRepository.GetShowByIdAsync(showId);
        if (show == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Show with ID {showId} not found");
        }

        if (show.ShowTime < DateTime.UtcNow)
        {
            throw new ArgumentException("Cannot book shows in the past");
        }

        var selectedSeats = await _seatRepository.GetSeatsByIdsAsync(selectedSeatIds);
        if (selectedSeats.Count != selectedSeatIds.Length)
        {
            throw new ArgumentException("One or more seat IDs are invalid");
        }

        if (selectedSeats.Any(s => s.ScreenId != show.ScreenId))
        {
            throw new ArgumentException("All seats must belong to the same screen as the show");
        }

        if (selectedSeats.Any(s => s.Price <= 0))
        {
            throw new InvalidOperationException("Invalid seat pricing configuration");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowAsync(showId);
            var unavailableSeats = selectedSeatIds.Where(id => bookedSeatIds.Contains(id)).ToList();

            if (unavailableSeats.Any())
            {
                throw new InvalidOperationException($"The following seats are no longer available: {string.Join(", ", unavailableSeats)}");
            }

            var totalAmount = selectedSeats.Sum(s => s.Price);

            var booking = new MovieBooking
            {
                UserId = userId,
                ShowId = showId,
                SelectedSeatIds = selectedSeatIds,
                TotalAmount = totalAmount,
                Status = "Confirmed",
                BookedAt = DateTime.UtcNow
            };

            booking = await _bookingRepository.CreateBookingAsync(booking);

            show.AvailableSeats -= selectedSeatIds.Length;
            await _showRepository.UpdateShowAsync(show);

            await transaction.CommitAsync();

            _logger.LogInformation("Booking {BookingId} created for user {UserId} on show {ShowId} with {SeatCount} seats",
                booking.Id, userId, showId, selectedSeatIds.Length);

            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                ShowId = booking.ShowId,
                SelectedSeatIds = booking.SelectedSeatIds,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                BookedAt = booking.BookedAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingDto> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Booking with ID {bookingId} not found");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this booking");
        }

        if (booking.Status == "Cancelled")
        {
            throw new InvalidOperationException("Booking is already cancelled");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            await _bookingRepository.UpdateBookingAsync(booking);

            var show = await _showRepository.GetShowByIdAsync(booking.ShowId);
            if (show != null)
            {
                show.AvailableSeats += booking.SelectedSeatIds.Length;
                await _showRepository.UpdateShowAsync(show);
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Booking {BookingId} cancelled for user {UserId}", bookingId, userId);

            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                ShowId = booking.ShowId,
                SelectedSeatIds = booking.SelectedSeatIds,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                BookedAt = booking.BookedAt,
                CancelledAt = booking.CancelledAt
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<AdminBookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return bookings.Select(b => new AdminBookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            ShowId = b.ShowId,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            BookedAt = b.BookedAt
        }).ToList();
    }

    public async Task<BookingStatsDto> GetBookingStatsAsync()
    {
        var bookings = await _bookingRepository.GetAllAsync();
        return new BookingStatsDto
        {
            Total = bookings.Count,
            Cancelled = bookings.Count(b => b.Status == "Cancelled")
        };
    }

    public async Task<BookingDto> GetBookingAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException($"Booking with ID {bookingId} not found");
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this booking");
        }

        var seats = await _seatRepository.GetSeatsByIdsAsync(booking.SelectedSeatIds);

        return new BookingDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            ShowId = booking.ShowId,
            SelectedSeatIds = booking.SelectedSeatIds,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            BookedAt = booking.BookedAt,
            CancelledAt = booking.CancelledAt,
            Show = booking.Show == null ? null : new ShowDto
            {
                Id = booking.Show.Id,
                MovieId = booking.Show.MovieId,
                ScreenId = booking.Show.ScreenId,
                ShowTime = booking.Show.ShowTime,
                AvailableSeats = booking.Show.AvailableSeats,
                Screen = booking.Show.Screen == null ? null : new ScreenDto
                {
                    Id = booking.Show.Screen.Id,
                    Name = booking.Show.Screen.Name,
                    TotalSeats = booking.Show.Screen.TotalSeats,
                    Cinema = booking.Show.Screen.Cinema == null ? null : new CinemaDto
                    {
                        Id = booking.Show.Screen.Cinema.Id,
                        Name = booking.Show.Screen.Cinema.Name,
                        City = booking.Show.Screen.Cinema.City,
                        Address = booking.Show.Screen.Cinema.Address
                    }
                }
            },
            Seats = seats.Select(s => new SeatDto
            {
                Id = s.Id,
                ScreenId = s.ScreenId,
                RowLabel = s.RowLabel,
                SeatNumber = s.SeatNumber,
                Category = s.Category,
                Price = s.Price,
                IsAvailable = false
            }).ToList()
        };
    }
}
