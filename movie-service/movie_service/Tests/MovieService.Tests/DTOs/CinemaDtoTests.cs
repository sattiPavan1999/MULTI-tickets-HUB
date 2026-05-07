using MovieService.Core.DTOs;

namespace MovieService.Tests.DTOs;

public class CinemaDtoTests
{
    [Fact]
    public void CinemaDto_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var dto = new CinemaDto
        {
            Id = 1,
            Name = "Cineplex Downtown",
            City = "Mumbai",
            Address = "123 Main Street"
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Cineplex Downtown", dto.Name);
        Assert.Equal("Mumbai", dto.City);
        Assert.Equal("123 Main Street", dto.Address);
    }

    [Fact]
    public void CinemaDto_Should_HandleDifferentCities()
    {
        // Arrange & Act
        var dto1 = new CinemaDto { Id = 1, Name = "Cinema 1", City = "Mumbai", Address = "Address 1" };
        var dto2 = new CinemaDto { Id = 2, Name = "Cinema 2", City = "Delhi", Address = "Address 2" };
        var dto3 = new CinemaDto { Id = 3, Name = "Cinema 3", City = "Bangalore", Address = "Address 3" };

        // Assert
        Assert.Equal("Mumbai", dto1.City);
        Assert.Equal("Delhi", dto2.City);
        Assert.Equal("Bangalore", dto3.City);
    }
}
