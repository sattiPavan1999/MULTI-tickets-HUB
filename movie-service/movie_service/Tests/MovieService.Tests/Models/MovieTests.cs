using MovieService.Core.Models;

namespace MovieService.Tests.Models;

public class MovieTests
{
    [Fact]
    public void Movie_Should_SetAndGetProperties()
    {
        // Arrange & Act
        var movie = new Movie
        {
            Id = 1,
            Title = "Inception",
            Genre = "Science Fiction",
            Language = "English",
            Format = "IMAX",
            DurationMinutes = 148,
            Synopsis = "A thief who steals corporate secrets",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal(1, movie.Id);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal("Science Fiction", movie.Genre);
        Assert.Equal("English", movie.Language);
        Assert.Equal("IMAX", movie.Format);
        Assert.Equal(148, movie.DurationMinutes);
        Assert.NotNull(movie.Synopsis);
        Assert.Equal("https://example.com/poster.jpg", movie.PosterUrl);
    }

    [Fact]
    public void Movie_Should_InitializeShowsCollection()
    {
        // Arrange & Act
        var movie = new Movie
        {
            Title = "Test",
            Genre = "Action",
            Language = "English",
            Format = "2D",
            DurationMinutes = 120,
            Synopsis = "Test"
        };

        // Assert
        Assert.NotNull(movie.Shows);
        Assert.Empty(movie.Shows);
    }

    [Theory]
    [InlineData("2D")]
    [InlineData("3D")]
    [InlineData("IMAX")]
    public void Movie_Should_AcceptValidFormats(string format)
    {
        // Arrange & Act
        var movie = new Movie
        {
            Title = "Test",
            Genre = "Action",
            Language = "English",
            Format = format,
            DurationMinutes = 120,
            Synopsis = "Test"
        };

        // Assert
        Assert.Equal(format, movie.Format);
    }
}
