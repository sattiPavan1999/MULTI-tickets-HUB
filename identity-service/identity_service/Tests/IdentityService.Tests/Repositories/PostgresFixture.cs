using IdentityService.Core.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace IdentityService.Tests.Repositories;

/// <summary>
/// Starts a single Postgres container and applies migrations once for the entire collection.
/// Individual tests share the container and clean their own data in InitializeAsync.
/// </summary>
[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
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
