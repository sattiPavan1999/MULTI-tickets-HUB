using MovieService.Core.DTOs;

namespace MovieService.Tests.DTOs;

public class MovieDtoTests
{
    [Fact]
    public void MovieDto_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var dto = new MovieDto
        {
            Id = 1,
            Title = "Test Movie",
            Genre = "Action",
            Language = "English",
            Format = "IMAX",
            DurationMinutes = 120,
            Synopsis = "Test synopsis",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal(1, dto.Id);
        Assert.Equal("Test Movie", dto.Title);
        Assert.Equal("Action", dto.Genre);
        Assert.Equal("English", dto.Language);
        Assert.Equal("IMAX", dto.Format);
        Assert.Equal(120, dto.DurationMinutes);
        Assert.Equal("Test synopsis", dto.Synopsis);
        Assert.Equal("https://example.com/poster.jpg", dto.PosterUrl);
    }

    [Fact]
    public void MovieDto_Should_AllowNullPosterUrl()
    {
        // Arrange & Act
        var dto = new MovieDto
        {
            Id = 1,
            Title = "Test Movie",
            Genre = "Action",
            Language = "English",
            Format = "2D",
            DurationMinutes = 90,
            Synopsis = "Test",
            PosterUrl = null
        };

        // Assert
        Assert.Null(dto.PosterUrl);
    }
}
