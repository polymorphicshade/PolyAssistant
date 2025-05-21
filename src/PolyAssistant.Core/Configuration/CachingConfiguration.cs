namespace PolyAssistant.Core.Configuration;

public sealed class CachingConfiguration
{
    public const string Key = "Caching";

    public DatabaseType DatabaseType { get; set; } = DatabaseType.Sqlite;

    public string ConnectionString { get; set; } = "Data Source=.\\cache.db";
}