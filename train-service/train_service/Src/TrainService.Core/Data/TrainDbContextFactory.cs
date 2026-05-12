using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TrainService.Core.Settings;

namespace TrainService.Core.Data;

public class TrainDbContextFactory : IDesignTimeDbContextFactory<TrainDbContext>
{
    public TrainDbContext CreateDbContext(string[] args)
    {
        // Reads from appsettings.json in the service root when running: cd train-service/train_service && dotnet ef ...
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Src/TrainService.Endpoints/appsettings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var settings = configuration.GetSection("TrainSettings:Core").Get<TrainCoreSettings>()
            ?? new TrainCoreSettings { Db = new DbConfig { DbSchema = "trains", ConnectionString = "Host=localhost;Port=5435;Database=trains_db;Username=postgres;Password=postgres" } };

        var optionsBuilder = new DbContextOptionsBuilder<TrainDbContext>();
        optionsBuilder.UseNpgsql(
            settings.Db.ConnectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", settings.Db.DbSchema)
        );

        return new TrainDbContext(optionsBuilder.Options, Options.Create(settings));
    }
}
