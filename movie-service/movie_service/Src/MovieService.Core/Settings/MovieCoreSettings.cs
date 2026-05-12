namespace MovieService.Core.Settings;

public class MovieCoreSettings
{
    public DbConfig Db { get; set; } = new();
}

public class DbConfig
{
    public string DbSchema { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
