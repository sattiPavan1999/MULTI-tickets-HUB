using MovieService.Core.DTOs;

namespace MovieService.Tests.DTOs;

public class ScreenDtoTests
{
    [Fact]
    public void ScreenDto_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var dto = new ScreenDto
        {
            Id = 1,
            Name = "Screen 1",
            TotalSeats = 100,
            Cinema = new CinemaDto
            {
                Id = 1,
                Name = "Cinema 1",
                City = "Mumbai",
                Address = "Address"
            }
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Screen 1", dto.Name);
        Assert.Equal(100, dto.TotalSeats);
        Assert.NotNull(dto.Cinema);
        Assert.Equal("Cinema 1", dto.Cinema.Name);
    }

    [Fact]
    public void ScreenDto_Should_AllowNullCinema()
    {
        // Arrange & Act
        var dto = new ScreenDto
        {
            Id = 1,
            Name = "Screen 1",
            TotalSeats = 100,
            Cinema = null
        };

        // Assert
        Assert.Null(dto.Cinema);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(200)]
    public void ScreenDto_Should_HandleDifferentCapacities(int totalSeats)
    {
        // Arrange & Act
        var dto = new ScreenDto
        {
            Id = 1,
            Name = $"Screen {totalSeats}",
            TotalSeats = totalSeats
        };

        // Assert
        Assert.Equal(totalSeats, dto.TotalSeats);
    }
}
