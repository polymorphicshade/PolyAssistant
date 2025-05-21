namespace PolyAssistant.Core;

public static class Constants
{
}

public enum DatabaseType
{
    /// <summary>
    ///     Example connection string:
    ///     <br />
    ///     <br />
    ///     server=localhost;database=my_database;user=username;password=password
    /// </summary>
    MySql,

    /// <summary>
    ///     Example connection string:
    ///     <br />
    ///     <br />
    ///     Data Source=.\path\to\database\file.db
    /// </summary>
    Sqlite,

    /// <summary>
    ///     Example connection string:
    ///     <br />
    ///     <br />
    ///     Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect
    ///     Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False
    /// </summary>
    SqlServer
}

public enum SearchQueryType
{
    General,
    News,
    Wikipedia
}

public enum ScrapeQueryType
{
    Html,
    Markdown
}