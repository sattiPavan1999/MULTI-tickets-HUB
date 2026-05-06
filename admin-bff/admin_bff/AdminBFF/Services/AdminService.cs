using AdminBFF.DTOs;
using AdminBFF.Models;

namespace AdminBFF.Services;

public class AdminService : IAdminService
{
    private readonly IIdentityService _identityService;
    private readonly ITrainService _trainService;
    private readonly IMovieService _movieService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IIdentityService identityService,
        ITrainService trainService,
        IMovieService movieService,
        ILogger<AdminService> logger)
    {
        _identityService = identityService;
        _trainService = trainService;
        _movieService = movieService;
        _logger = logger;
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _identityService.GetUserByIdAsync(userId);

        if (user.Role != "Admin")
        {
            throw new ForbiddenException("Admin access required");
        }

        return user;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _identityService.GetAllUsersAsync();
    }

    public async Task<OperationResultDto> DeactivateUserAsync(int userId, int currentUserId)
    {
        if (userId == currentUserId)
        {
            throw new ValidationException("Cannot deactivate your own account");
        }

        return await _identityService.DeactivateUserAsync(userId);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync(BookingFilterInput? filter)
    {
        // Fetch bookings from both services in parallel
        var trainBookingsTask = _trainService.GetAllBookingsAsync();
        var movieBookingsTask = _movieService.GetAllBookingsAsync();

        await Task.WhenAll(trainBookingsTask, movieBookingsTask);

        var allBookings = new List<BookingDto>();
        allBookings.AddRange(await trainBookingsTask);
        allBookings.AddRange(await movieBookingsTask);

        // Apply filters if provided
        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Status))
            {
                allBookings = allBookings.Where(b => b.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(filter.ServiceType))
            {
                allBookings = allBookings.Where(b => b.BookingType.Equals(filter.ServiceType, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        return allBookings.OrderByDescending(b => b.BookedAt).ToList();
    }

    public async Task<OperationResultDto> CancelBookingAsync(int bookingId, string bookingType)
    {
        // Validate booking type
        if (bookingType != "Train" && bookingType != "Movie")
        {
            throw new ValidationException("bookingType must be 'Train' or 'Movie'");
        }

        // Route to appropriate service
        if (bookingType == "Train")
        {
            return await _trainService.CancelBookingAsync(bookingId);
        }
        else
        {
            return await _movieService.CancelBookingAsync(bookingId);
        }
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        // Fetch stats from all services in parallel
        var userCountTask = _identityService.GetActiveUserCountAsync();
        var trainStatsTask = _trainService.GetBookingStatsAsync();
        var movieStatsTask = _movieService.GetBookingStatsAsync();

        await Task.WhenAll(userCountTask, trainStatsTask, movieStatsTask);

        var activeUsers = await userCountTask;
        var trainStats = await trainStatsTask;
        var movieStats = await movieStatsTask;

        var totalBookings = (trainStats.GetValueOrDefault("total", 0) + movieStats.GetValueOrDefault("total", 0));
        var cancellationCount = (trainStats.GetValueOrDefault("cancelled", 0) + movieStats.GetValueOrDefault("cancelled", 0));

        return new DashboardStatsDto
        {
            TotalBookings = totalBookings,
            ActiveUsers = activeUsers,
            CancellationCount = cancellationCount
        };
    }

    public async Task<TrainDto> AddTrainAsync(AddTrainInput input)
    {
        // Validate departure and arrival times
        if (TimeSpan.TryParse(input.DepartureTime, out var departure) &&
            TimeSpan.TryParse(input.ArrivalTime, out var arrival))
        {
            if (departure >= arrival)
            {
                throw new ValidationException("Departure time must be before arrival time");
            }
        }

        return await _trainService.AddTrainAsync(input);
    }

    public async Task<MovieDto> AddMovieAsync(AddMovieInput input)
    {
        // Validate movie format
        var validFormats = new[] { "2D", "3D", "IMAX" };
        if (!validFormats.Contains(input.Format, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException("format must be 2D, 3D, or IMAX");
        }

        return await _movieService.AddMovieAsync(input);
    }
}
