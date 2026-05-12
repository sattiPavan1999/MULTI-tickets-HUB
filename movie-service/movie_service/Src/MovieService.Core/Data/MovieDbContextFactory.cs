using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MovieService.Core.Settings;

namespace MovieService.Core.Data;

public class MovieDbContextFactory : IDesignTimeDbContextFactory<MovieDbContext>
{
    public MovieDbContext CreateDbContext(string[] args)
    {
        // Reads from appsettings.json in the service root when running: cd movie-service/movie_service && dotnet ef ...
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Src/MovieService.Endpoints/appsettings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var settings = configuration.GetSection("MovieSettings:Core").Get<MovieCoreSettings>()
            ?? new MovieCoreSettings { Db = new DbConfig { DbSchema = "movies", ConnectionString = "Host=localhost;Port=5435;Database=movies_db;Username=postgres;Password=postgres" } };

        var optionsBuilder = new DbContextOptionsBuilder<MovieDbContext>();
        optionsBuilder.UseNpgsql(
            settings.Db.ConnectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", settings.Db.DbSchema)
        );

        return new MovieDbContext(optionsBuilder.Options, Options.Create(settings));
    }
}
