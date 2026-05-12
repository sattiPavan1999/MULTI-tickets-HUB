using IdentityService.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace IdentityService.Core.Data;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        // Reads from appsettings.json in the service root when running: cd identity-service/identity_service && dotnet ef ...
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Src/IdentityService.Endpoints/appsettings.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var settings = configuration.GetSection("IdentitySettings:Core").Get<IdentityCoreSettings>()
            ?? new IdentityCoreSettings { Db = new DbConfig { DbSchema = "identity", ConnectionString = "Host=localhost;Port=5435;Database=identity_db;Username=postgres;Password=postgres" } };

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(
            settings.Db.ConnectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", settings.Db.DbSchema)
        );

        return new IdentityDbContext(optionsBuilder.Options, Options.Create(settings));
    }
}
