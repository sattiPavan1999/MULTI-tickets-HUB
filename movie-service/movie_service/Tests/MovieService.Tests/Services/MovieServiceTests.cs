using AutoMapper;
using Bogus;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieService.Core.Data;
using MovieService.Core.DTOs;
using MovieService.Core.Exceptions;
using MovieService.Core.Mapping;
using MovieService.Core.Models;
using MovieService.Core.Repositories;
using MovieService.Core.Services;
using MovieService.Core.Validators;

namespace MovieService.Tests.Services;

public class MovieServiceTests
{
    private static readonly Faker Fake = new();

    private static IMapper BuildMapper()
        => new MapperConfiguration(c => c.AddProfile<MovieMappingProfile>()).CreateMapper();

    private static (IMovieService svc, MovieDbContext db) BuildFullService(string dbName)
    {
        var options = new DbContextOptionsBuilder<MovieDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var db = new MovieDbContext(options);
        var repo = new MovieRepository(db, NullLogger<MovieRepository>.Instance);
        var svc = new MovieService.Core.Services.MovieService(
            repo,
            new CreateMovieInputValidator(),
            new UpdateMovieInputValidator(),
            BuildMapper(),
            NullLogger<MovieService.Core.Services.MovieService>.Instance);
        return (svc, db);
    }

    private static IMovieService BuildMocked(Mock<IMovieRepository> repo)
    {
        return new MovieService.Core.Services.MovieService(
            repo.Object,
            new CreateMovieInputValidator(),
            new UpdateMovieInputValidator(),
            BuildMapper(),
            NullLogger<MovieService.Core.Services.MovieService>.Instance);
    }

    private static Movie MakeEntity() => new()
    {
        Id = Fake.Random.Int(1, 1000),
        Title = Fake.Lorem.Word(),
        Genre = "Action",
        Duration = 120,
        PosterUrl = "https://example.com/poster.jpg",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static CreateMovieInput ValidCreateInput() => new()
    {
        Title = "Inception",
        Genre = "Sci-Fi",
        Duration = 148,
        PosterUrl = "https://example.com/poster.jpg"
    };

    // ── CreateMovie ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMovie_ValidInput_ReturnsMovieResponse()
    {
        var (svc, _) = BuildFullService(nameof(CreateMovie_ValidInput_ReturnsMovieResponse));

        var result = await svc.CreateMovieAsync(ValidCreateInput());

        result.Title.Should().Be("Inception");
        result.IsActive.Should().BeTrue();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateMovie_EmptyTitle_ThrowsValidationException()
    {
        var (svc, _) = BuildFullService(nameof(CreateMovie_EmptyTitle_ThrowsValidationException));
        var input = ValidCreateInput();
        input.Title = "";

        await svc.Invoking(s => s.CreateMovieAsync(input))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task CreateMovie_ZeroDuration_ThrowsValidationException()
    {
        var (svc, _) = BuildFullService(nameof(CreateMovie_ZeroDuration_ThrowsValidationException));
        var input = ValidCreateInput();
        input.Duration = 0;

        await svc.Invoking(s => s.CreateMovieAsync(input))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    // ── GetAllMovies ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllMovies_ReturnsAllMovies()
    {
        var repo = new Mock<IMovieRepository>();
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([MakeEntity(), MakeEntity(), MakeEntity()]);
        var svc = BuildMocked(repo);

        var result = await svc.GetAllMoviesAsync();

        result.Should().HaveCount(3);
    }

    // ── GetMovieById ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMovieById_ExistingId_ReturnsResponse()
    {
        var entity = MakeEntity();
        var repo = new Mock<IMovieRepository>();
        repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var svc = BuildMocked(repo);

        var result = await svc.GetMovieByIdAsync(entity.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetMovieById_NonExistentId_ReturnsNull()
    {
        var repo = new Mock<IMovieRepository>();
        repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);
        var svc = BuildMocked(repo);

        var result = await svc.GetMovieByIdAsync(99);

        result.Should().BeNull();
    }

    // ── UpdateMovie ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMovie_UpdatesTitle()
    {
        var (svc, db) = BuildFullService(nameof(UpdateMovie_UpdatesTitle));
        var created = await svc.CreateMovieAsync(ValidCreateInput());

        var result = await svc.UpdateMovieAsync(created.Id, new UpdateMovieInput { Title = "Updated Title" });

        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateMovie_UnknownId_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(UpdateMovie_UnknownId_ThrowsNotFoundException));

        await svc.Invoking(s => s.UpdateMovieAsync(9999, new UpdateMovieInput { Title = "X" }))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ── DeleteMovie ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMovie_RemovesMovie()
    {
        var (svc, db) = BuildFullService(nameof(DeleteMovie_RemovesMovie));
        var created = await svc.CreateMovieAsync(ValidCreateInput());

        await svc.DeleteMovieAsync(created.Id);

        var found = await svc.GetMovieByIdAsync(created.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMovie_UnknownId_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(DeleteMovie_UnknownId_ThrowsNotFoundException));

        await svc.Invoking(s => s.DeleteMovieAsync(9999))
            .Should().ThrowAsync<NotFoundException>();
    }

    // ── ToggleMovieStatus ─────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleMovieStatus_ActiveMovie_DeactivatesIt()
    {
        var (svc, db) = BuildFullService(nameof(ToggleMovieStatus_ActiveMovie_DeactivatesIt));
        var created = await svc.CreateMovieAsync(ValidCreateInput());

        var result = await svc.ToggleMovieStatusAsync(created.Id);

        result.Success.Should().BeTrue();
        var refreshed = db.Movies.Find(created.Id);
        refreshed!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleMovieStatus_UnknownId_ThrowsNotFoundException()
    {
        var (svc, _) = BuildFullService(nameof(ToggleMovieStatus_UnknownId_ThrowsNotFoundException));

        await svc.Invoking(s => s.ToggleMovieStatusAsync(9999))
            .Should().ThrowAsync<NotFoundException>();
    }
}
