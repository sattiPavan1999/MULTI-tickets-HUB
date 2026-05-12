namespace TrainService.Core.Settings;

public class TrainCoreSettings
{
    public DbConfig Db { get; set; } = new();
}

public class DbConfig
{
    public string DbSchema { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
