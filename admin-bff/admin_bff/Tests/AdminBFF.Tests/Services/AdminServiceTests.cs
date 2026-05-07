using AdminBFF.Core.DTOs;
using AdminBFF.Core.Models;
using AdminBFF.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdminBFF.Tests.Services;

public class AdminServiceTests
{
    private class TestIdentityService : IIdentityService
    {
        public UserDto? UserToReturn { get; set; }
        public List<UserDto> UsersToReturn { get; set; } = new();
        public int ActiveUserCount { get; set; }
        public bool ShouldThrowException { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<UserDto> GetUserByIdAsync(int userId)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(UserToReturn ?? new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User",
                PhoneNumber = "+1234567890",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            });
        }

        public Task<List<UserDto>> GetAllUsersAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(UsersToReturn);
        }

        public Task<int> GetActiveUserCountAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(ActiveUserCount);
        }

        public Task<OperationResultDto> DeactivateUserAsync(int userId)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(new OperationResultDto
            {
                Success = true,
                Message = "User deactivated successfully"
            });
        }
    }

    private class TestTrainService : ITrainService
    {
        public List<BookingDto> BookingsToReturn { get; set; } = new();
        public Dictionary<string, int> StatsToReturn { get; set; } = new();
        public bool ShouldThrowException { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<List<BookingDto>> GetAllBookingsAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(BookingsToReturn);
        }

        public Task<Dictionary<string, int>> GetBookingStatsAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(StatsToReturn);
        }

        public Task<OperationResultDto> CancelBookingAsync(int bookingId)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(new OperationResultDto
            {
                Success = true,
                Message = "Booking cancelled successfully"
            });
        }

        public Task<TrainDto> AddTrainAsync(AddTrainInput input)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(new TrainDto
            {
                Id = 1,
                TrainNumber = input.TrainNumber,
                TrainName = input.TrainName,
                SourceStation = input.SourceStation,
                DestinationStation = input.DestinationStation,
                DepartureTime = input.DepartureTime,
                ArrivalTime = input.ArrivalTime,
                TotalSeats = input.TotalSeats
            });
        }
    }

    private class TestMovieService : IMovieService
    {
        public List<BookingDto> BookingsToReturn { get; set; } = new();
        public Dictionary<string, int> StatsToReturn { get; set; } = new();
        public bool ShouldThrowException { get; set; }
        public Exception? ExceptionToThrow { get; set; }

        public Task<List<BookingDto>> GetAllBookingsAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(BookingsToReturn);
        }

        public Task<Dictionary<string, int>> GetBookingStatsAsync()
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(StatsToReturn);
        }

        public Task<OperationResultDto> CancelBookingAsync(int bookingId)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(new OperationResultDto
            {
                Success = true,
                Message = "Booking cancelled successfully"
            });
        }

        public Task<MovieDto> AddMovieAsync(AddMovieInput input)
        {
            if (ShouldThrowException && ExceptionToThrow != null)
                throw ExceptionToThrow;

            return Task.FromResult(new MovieDto
            {
                Id = 1,
                Title = input.Title,
                Genre = input.Genre,
                Language = input.Language,
                Format = input.Format,
                DurationMinutes = input.DurationMinutes,
                Synopsis = input.Synopsis,
                PosterUrl = input.PosterUrl
            });
        }
    }

    [Fact]
    public async Task GetCurrentUserAsync_Should_Return_Admin_User()
    {
        // Arrange
        var identityService = new TestIdentityService
        {
            UserToReturn = new UserDto
            {
                Id = 1,
                Email = "admin@example.com",
                FullName = "Admin User",
                PhoneNumber = "+1234567890",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        };
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetCurrentUserAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task GetCurrentUserAsync_Should_Throw_Forbidden_For_Non_Admin()
    {
        // Arrange
        var identityService = new TestIdentityService
        {
            UserToReturn = new UserDto
            {
                Id = 1,
                Email = "user@example.com",
                FullName = "Regular User",
                PhoneNumber = "+1234567890",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            }
        };
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetCurrentUserAsync(1));
    }

    [Fact]
    public async Task GetAllUsersAsync_Should_Return_All_Users()
    {
        // Arrange
        var identityService = new TestIdentityService
        {
            UsersToReturn = new List<UserDto>
            {
                new() { Id = 1, Email = "user1@example.com", FullName = "User 1", PhoneNumber = "+1234567890", Role = "User", CreatedAt = DateTime.UtcNow },
                new() { Id = 2, Email = "user2@example.com", FullName = "User 2", PhoneNumber = "+1234567890", Role = "Admin", CreatedAt = DateTime.UtcNow }
            }
        };
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task DeactivateUserAsync_Should_Prevent_Self_Deactivation()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.DeactivateUserAsync(1, 1));
        Assert.Equal("Cannot deactivate your own account", exception.Message);
    }

    [Fact]
    public async Task DeactivateUserAsync_Should_Succeed_For_Different_User()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.DeactivateUserAsync(5, 1);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetAllBookingsAsync_Should_Aggregate_Train_And_Movie_Bookings()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService
        {
            BookingsToReturn = new List<BookingDto>
            {
                new() { Id = 1, UserId = 1, BookingType = "Train", Pnr = 123, ShowId = null, TotalAmount = 100, Status = "Confirmed", BookedAt = DateTime.UtcNow }
            }
        };
        var movieService = new TestMovieService
        {
            BookingsToReturn = new List<BookingDto>
            {
                new() { Id = 2, UserId = 2, BookingType = "Movie", Pnr = null, ShowId = 45, TotalAmount = 200, Status = "Confirmed", BookedAt = DateTime.UtcNow }
            }
        };
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetAllBookingsAsync(null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.BookingType == "Train");
        Assert.Contains(result, b => b.BookingType == "Movie");
    }

    [Fact]
    public async Task GetAllBookingsAsync_Should_Filter_By_Status()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService
        {
            BookingsToReturn = new List<BookingDto>
            {
                new() { Id = 1, UserId = 1, BookingType = "Train", Pnr = 123, ShowId = null, TotalAmount = 100, Status = "Confirmed", BookedAt = DateTime.UtcNow },
                new() { Id = 2, UserId = 1, BookingType = "Train", Pnr = 124, ShowId = null, TotalAmount = 100, Status = "Cancelled", BookedAt = DateTime.UtcNow }
            }
        };
        var movieService = new TestMovieService { BookingsToReturn = new List<BookingDto>() };
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetAllBookingsAsync(new BookingFilterInput { Status = "Confirmed" });

        // Assert
        Assert.Single(result);
        Assert.Equal("Confirmed", result[0].Status);
    }

    [Fact]
    public async Task GetAllBookingsAsync_Should_Filter_By_ServiceType()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService
        {
            BookingsToReturn = new List<BookingDto>
            {
                new() { Id = 1, UserId = 1, BookingType = "Train", Pnr = 123, ShowId = null, TotalAmount = 100, Status = "Confirmed", BookedAt = DateTime.UtcNow }
            }
        };
        var movieService = new TestMovieService
        {
            BookingsToReturn = new List<BookingDto>
            {
                new() { Id = 2, UserId = 2, BookingType = "Movie", Pnr = null, ShowId = 45, TotalAmount = 200, Status = "Confirmed", BookedAt = DateTime.UtcNow }
            }
        };
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetAllBookingsAsync(new BookingFilterInput { ServiceType = "Train" });

        // Assert
        Assert.Single(result);
        Assert.Equal("Train", result[0].BookingType);
    }

    [Fact]
    public async Task CancelBookingAsync_Should_Route_To_Train_Service()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.CancelBookingAsync(101, "Train");

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CancelBookingAsync_Should_Route_To_Movie_Service()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.CancelBookingAsync(202, "Movie");

        // Assert
        Assert.True(result.Success);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("TRAIN")]
    public async Task CancelBookingAsync_Should_Throw_ValidationException_For_Invalid_Type(string bookingType)
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.CancelBookingAsync(1, bookingType));
    }

    [Fact]
    public async Task GetDashboardStatsAsync_Should_Aggregate_All_Stats()
    {
        // Arrange
        var identityService = new TestIdentityService { ActiveUserCount = 100 };
        var trainService = new TestTrainService
        {
            StatsToReturn = new Dictionary<string, int> { ["total"] = 50, ["cancelled"] = 5 }
        };
        var movieService = new TestMovieService
        {
            StatsToReturn = new Dictionary<string, int> { ["total"] = 30, ["cancelled"] = 3 }
        };
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        // Act
        var result = await service.GetDashboardStatsAsync();

        // Assert
        Assert.Equal(80, result.TotalBookings);
        Assert.Equal(100, result.ActiveUsers);
        Assert.Equal(8, result.CancellationCount);
    }

    [Fact]
    public async Task AddTrainAsync_Should_Validate_Departure_Before_Arrival()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        var input = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "A",
            DestinationStation = "B",
            DepartureTime = "18:00",
            ArrivalTime = "10:00",
            TotalSeats = new Dictionary<string, int>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.AddTrainAsync(input));
    }

    [Fact]
    public async Task AddTrainAsync_Should_Succeed_With_Valid_Times()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        var input = new AddTrainInput
        {
            TrainNumber = "12345",
            TrainName = "Express",
            SourceStation = "A",
            DestinationStation = "B",
            DepartureTime = "10:00",
            ArrivalTime = "18:00",
            TotalSeats = new Dictionary<string, int>()
        };

        // Act
        var result = await service.AddTrainAsync(input);

        // Assert
        Assert.Equal("12345", result.TrainNumber);
    }

    [Theory]
    [InlineData("2D")]
    [InlineData("3D")]
    [InlineData("IMAX")]
    public async Task AddMovieAsync_Should_Accept_Valid_Formats(string format)
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        var input = new AddMovieInput
        {
            Title = "Test Movie",
            Genre = "Action",
            Language = "English",
            Format = format,
            DurationMinutes = 120,
            Synopsis = "Test",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Act
        var result = await service.AddMovieAsync(input);

        // Assert
        Assert.Equal(format, result.Format);
    }

    [Fact]
    public async Task AddMovieAsync_Should_Reject_Invalid_Format()
    {
        // Arrange
        var identityService = new TestIdentityService();
        var trainService = new TestTrainService();
        var movieService = new TestMovieService();
        var service = new AdminService(identityService, trainService, movieService, NullLogger<AdminService>.Instance);

        var input = new AddMovieInput
        {
            Title = "Test Movie",
            Genre = "Action",
            Language = "English",
            Format = "4D",
            DurationMinutes = 120,
            Synopsis = "Test",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.AddMovieAsync(input));
    }
}
