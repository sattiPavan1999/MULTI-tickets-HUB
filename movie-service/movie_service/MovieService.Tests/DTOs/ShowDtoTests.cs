using MovieService.DTOs;

namespace MovieService.Tests.DTOs;

public class ShowDtoTests
{
    [Fact]
    public void ShowDto_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var screen = new ScreenDto
        {
            Id = 1,
            Name = "Screen 1",
            TotalSeats = 100,
            Cinema = new CinemaDto
            {
                Id = 1,
                Name = "Cinema 1",
                City = "City",
                Address = "Address"
            }
        };

        var dto = new ShowDto
        {
            Id = 1,
            MovieId = 10,
            ScreenId = 5,
            ShowTime = new DateTime(2026, 5, 15, 18, 30, 0, DateTimeKind.Utc),
            AvailableSeats = 50,
            Screen = screen
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal(10, dto.MovieId);
        Assert.Equal(5, dto.ScreenId);
        Assert.Equal(new DateTime(2026, 5, 15, 18, 30, 0, DateTimeKind.Utc), dto.ShowTime);
        Assert.Equal(50, dto.AvailableSeats);
        Assert.NotNull(dto.Screen);
        Assert.Equal("Screen 1", dto.Screen.Name);
    }

    [Fact]
    public void ShowDto_Should_AllowNullScreen()
    {
        // Arrange & Act
        var dto = new ShowDto
        {
            Id = 1,
            MovieId = 10,
            ScreenId = 5,
            ShowTime = DateTime.UtcNow,
            AvailableSeats = 50,
            Screen = null
        };

        // Assert
        Assert.Null(dto.Screen);
    }
}
