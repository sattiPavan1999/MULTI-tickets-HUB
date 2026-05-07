using Microsoft.Extensions.Logging;
using MovieService.Core.Models;
using MovieService.Core.Repositories;
using MovieService.Core.Services;

namespace MovieService.Tests.Services;

public class MovieServiceTests
{
    private class MockMovieRepository : IMovieRepository
    {
        private readonly List<Movie> _movies;

        public MockMovieRepository(List<Movie> movies)
        {
            _movies = movies;
        }

        public Task<List<Movie>> GetMoviesAsync(string? genre, string? language, string? format)
        {
            var query = _movies.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(m => m.Genre == genre);

            if (!string.IsNullOrEmpty(language))
                query = query.Where(m => m.Language == language);

            if (!string.IsNullOrEmpty(format))
                query = query.Where(m => m.Format == format);

            return Task.FromResult(query.ToList());
        }

        public Task<Movie?> GetMovieByIdAsync(int id)
        {
            return Task.FromResult(_movies.FirstOrDefault(m => m.Id == id));
        }
    }

    private class MockLogger : ILogger<MovieServiceImpl>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    [Fact]
    public async Task GetMoviesAsync_Should_ReturnAllMovies_WhenNoFiltersProvided()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new() { Id = 1, Title = "Movie 1", Genre = "Action", Language = "English", Format = "2D", DurationMinutes = 120, Synopsis = "Test" },
            new() { Id = 2, Title = "Movie 2", Genre = "Drama", Language = "English", Format = "3D", DurationMinutes = 90, Synopsis = "Test" }
        };
        var repository = new MockMovieRepository(movies);
        var service = new MovieServiceImpl(repository, new MockLogger());

        // Act
        var result = await service.GetMoviesAsync(null, null, null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetMoviesAsync_Should_FilterByGenre()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new() { Id = 1, Title = "Movie 1", Genre = "Action", Language = "English", Format = "2D", DurationMinutes = 120, Synopsis = "Test" },
            new() { Id = 2, Title = "Movie 2", Genre = "Drama", Language = "English", Format = "3D", DurationMinutes = 90, Synopsis = "Test" }
        };
        var repository = new MockMovieRepository(movies);
        var service = new MovieServiceImpl(repository, new MockLogger());

        // Act
        var result = await service.GetMoviesAsync("Action", null, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("Action", result[0].Genre);
    }

    [Theory]
    [InlineData("2D")]
    [InlineData("3D")]
    [InlineData("IMAX")]
    public async Task GetMoviesAsync_Should_AcceptValidFormats(string format)
    {
        // Arrange
        var movies = new List<Movie>
        {
            new() { Id = 1, Title = "Movie 1", Genre = "Action", Language = "English", Format = format, DurationMinutes = 120, Synopsis = "Test" }
        };
        var repository = new MockMovieRepository(movies);
        var service = new MovieServiceImpl(repository, new MockLogger());

        // Act
        var result = await service.GetMoviesAsync(null, null, format);

        // Assert
        Assert.Single(result);
        Assert.Equal(format, result[0].Format);
    }

    [Fact]
    public async Task GetMoviesAsync_Should_ThrowException_WhenInvalidFormatProvided()
    {
        // Arrange
        var repository = new MockMovieRepository(new List<Movie>());
        var service = new MovieServiceImpl(repository, new MockLogger());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetMoviesAsync(null, null, "4D"));
    }
}
