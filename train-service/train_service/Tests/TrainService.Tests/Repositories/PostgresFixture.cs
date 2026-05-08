using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TrainService.Core.Data;

namespace TrainService.Tests.Repositories;

[CollectionDefinition("postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }

public class PostgresFixture : IAsyncLifetime
{
    private static readonly string DockerEndpoint =
        Environment.GetEnvironmentVariable("DOCKER_HOST")
        ?? "unix:///var/run/docker.sock";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDockerEndpoint(DockerEndpoint)
        .WithImage("postgres:17-alpine")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await _container.WaitForPort();
        ConnectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<TrainDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var ctx = new TrainDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
