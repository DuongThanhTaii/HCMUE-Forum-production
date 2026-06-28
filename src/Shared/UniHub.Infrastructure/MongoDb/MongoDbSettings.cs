namespace UniHub.Infrastructure.MongoDb;

/// <summary>
/// Configuration settings for MongoDB connection.
/// </summary>
public sealed class MongoDbSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "MongoDbSettings";

    /// <summary>
    /// Gets or sets the MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the server selection timeout in seconds.
    /// </summary>
    public int ServerSelectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum connection pool size.
    /// </summary>
    public int MaxConnectionPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum connection pool size.
    /// </summary>
    public int MinConnectionPoolSize { get; set; } = 0;
}
