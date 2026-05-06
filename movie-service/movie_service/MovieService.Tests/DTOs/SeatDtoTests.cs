using MovieService.DTOs;

namespace MovieService.Tests.DTOs;

public class SeatDtoTests
{
    [Fact]
    public void SeatDto_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var dto = new SeatDto
        {
            Id = 1,
            ScreenId = 5,
            RowLabel = "A",
            SeatNumber = 10,
            Category = "Regular",
            Price = 200.00m,
            IsAvailable = true
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal(5, dto.ScreenId);
        Assert.Equal("A", dto.RowLabel);
        Assert.Equal(10, dto.SeatNumber);
        Assert.Equal("Regular", dto.Category);
        Assert.Equal(200.00m, dto.Price);
        Assert.True(dto.IsAvailable);
    }

    [Theory]
    [InlineData("Regular", 200.00)]
    [InlineData("Premium", 350.00)]
    [InlineData("Recliner", 500.00)]
    public void SeatDto_Should_HandleDifferentCategories(string category, decimal price)
    {
        // Arrange & Act
        var dto = new SeatDto
        {
            Id = 1,
            ScreenId = 5,
            RowLabel = "A",
            SeatNumber = 1,
            Category = category,
            Price = price,
            IsAvailable = true
        };

        // Assert
        Assert.Equal(category, dto.Category);
        Assert.Equal(price, dto.Price);
    }
}
