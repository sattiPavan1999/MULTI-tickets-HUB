using AdminBFF.Models;

namespace AdminBFF.Tests.Models;

public class AddMovieInputTests
{
    [Fact]
    public void AddMovieInput_Should_Initialize_With_Valid_Values()
    {
        // Arrange & Act
        var input = new AddMovieInput
        {
            Title = "New Movie",
            Genre = "Action",
            Language = "English",
            Format = "IMAX",
            DurationMinutes = 150,
            Synopsis = "An action-packed thriller",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal("New Movie", input.Title);
        Assert.Equal("Action", input.Genre);
        Assert.Equal("English", input.Language);
        Assert.Equal("IMAX", input.Format);
        Assert.Equal(150, input.DurationMinutes);
        Assert.Equal("An action-packed thriller", input.Synopsis);
        Assert.Equal("https://example.com/poster.jpg", input.PosterUrl);
    }

    [Theory]
    [InlineData("2D")]
    [InlineData("3D")]
    [InlineData("IMAX")]
    public void AddMovieInput_Should_Accept_Valid_Formats(string format)
    {
        // Act
        var input = new AddMovieInput
        {
            Title = "Test Movie",
            Genre = "Drama",
            Language = "English",
            Format = format,
            DurationMinutes = 120,
            Synopsis = "Test synopsis",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal(format, input.Format);
    }

    [Fact]
    public void AddMovieInput_Should_Be_Immutable()
    {
        // Arrange
        var original = new AddMovieInput
        {
            Title = "Original Movie",
            Genre = "Action",
            Language = "English",
            Format = "2D",
            DurationMinutes = 120,
            Synopsis = "Original synopsis",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Act
        var modified = original with { Title = "Modified Movie" };

        // Assert
        Assert.Equal("Original Movie", original.Title);
        Assert.Equal("Modified Movie", modified.Title);
        Assert.Equal(original.Genre, modified.Genre);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(180)]
    [InlineData(240)]
    public void AddMovieInput_Should_Accept_Various_Durations(int duration)
    {
        // Act
        var input = new AddMovieInput
        {
            Title = "Test Movie",
            Genre = "Drama",
            Language = "English",
            Format = "2D",
            DurationMinutes = duration,
            Synopsis = "Test synopsis",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal(duration, input.DurationMinutes);
    }

    [Theory]
    [InlineData("Action", "English")]
    [InlineData("Drama", "Hindi")]
    [InlineData("Comedy", "Spanish")]
    [InlineData("Thriller", "French")]
    public void AddMovieInput_Should_Accept_Various_Genres_And_Languages(string genre, string language)
    {
        // Act
        var input = new AddMovieInput
        {
            Title = "Test Movie",
            Genre = genre,
            Language = language,
            Format = "2D",
            DurationMinutes = 120,
            Synopsis = "Test synopsis",
            PosterUrl = "https://example.com/poster.jpg"
        };

        // Assert
        Assert.Equal(genre, input.Genre);
        Assert.Equal(language, input.Language);
    }
}
