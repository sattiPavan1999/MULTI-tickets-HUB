using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using AdminBFF.Core.DTOs;
using AdminBFF.Core.Services;
using AdminBFF.Endpoints.GraphQL;

namespace AdminBFF.Tests.GraphQL;

public class QueryTests
{
    private static readonly Faker Fake = new();

    private static IHttpContextAccessor BuildAccessor(string? token = "test-token")
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        if (token is not null)
            context.Request.Headers.Authorization = $"Bearer {token}";
        accessor.Setup(a => a.HttpContext).Returns(context);
        return accessor.Object;
    }

    [Fact]
    public async Task GetUsers_ReturnsUserList()
    {
        var identitySvc = new Mock<IIdentityService>();
        identitySvc.Setup(s => s.GetAllUsersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UserDto { Id = 1, Email = "a@b.com", FullName = "Admin", Role = "Admin", IsActive = true }]);
        var query = new Query();

        var result = await query.GetUsers(identitySvc.Object, BuildAccessor(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Email.Should().Be("a@b.com");
    }

    [Fact]
    public async Task GetMovies_ReturnsMovieList()
    {
        var movieSvc = new Mock<IMovieService>();
        movieSvc.Setup(s => s.GetAllMoviesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new MovieDto { Id = 1, Title = "Inception", Genre = "Sci-Fi", Duration = 148, PosterUrl = "https://e.com/p.jpg", IsActive = true }]);
        var query = new Query();

        var result = await query.GetMovies(movieSvc.Object, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Inception");
    }

    [Fact]
    public async Task GetTrains_ReturnsTrainList()
    {
        var trainSvc = new Mock<ITrainService>();
        trainSvc.Setup(s => s.GetAllTrainsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new TrainDto { Id = 1, TrainName = "Rajdhani", TrainNumber = "12301", Source = "Delhi", Destination = "Howrah" }]);
        var query = new Query();

        var result = await query.GetTrains(trainSvc.Object, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].TrainNumber.Should().Be("12301");
    }

    [Fact]
    public async Task GetUsers_ForwardsToken_ToIdentityService()
    {
        var identitySvc = new Mock<IIdentityService>();
        identitySvc.Setup(s => s.GetAllUsersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var query = new Query();

        await query.GetUsers(identitySvc.Object, BuildAccessor("my-token"), CancellationToken.None);

        identitySvc.Verify(s => s.GetAllUsersAsync("my-token", It.IsAny<CancellationToken>()), Times.Once);
    }
}
