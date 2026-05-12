using IdentityService.Core.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace IdentityService.Tests.Repositories;

/// <summary>
/// Starts a single Postgres container and applies migrations once for the entire collection.
/// Individual tests share the container and clean their own data in InitializeAsync.
/// The Docker endpoint is resolved from DOCKER_HOST (falls back to /var/run/docker.sock).
/// </summary>
[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }

public class PostgresFixture : IAsyncLifetime
{
    // Resolve Docker socket from env var so Colima, Docker Desktop, and CI all work
    private static readonly string DockerEndpoint =
        Environment.GetEnvironmentVariable("DOCKER_HOST")
        ?? "unix:///var/run/docker.sock";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDockerEndpoint(DockerEndpoint)
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await _container.WaitForPort();
        ConnectionString = _container.GetConnectionString();

        // Apply migrations once — test classes only clean data, not re-migrate
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var ctx = new IdentityDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
