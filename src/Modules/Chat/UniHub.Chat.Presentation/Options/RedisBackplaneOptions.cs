namespace UniHub.Chat.Presentation.Options;

/// <summary>
/// Configuration options for Redis backplane used by SignalR.
/// Enables horizontal scaling across multiple server instances.
/// </summary>
public sealed class RedisBackplaneOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "RedisBackplane";

    /// <summary>
    /// Redis connection string. If null or empty, backplane is disabled and in-memory mode is used.
    /// Format: "localhost:6379" or "localhost:6379,password=yourpassword,ssl=true"
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Enable Redis backplane for SignalR. Default is true if ConnectionString is provided.
    /// Set to false to force in-memory mode even if Redis connection string exists.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis key prefix for SignalR messages. Default is "signalr:".
    /// Useful for multi-tenant scenarios or separating environments.
    /// </summary>
    public string KeyPrefix { get; set; } = "signalr:";

    /// <summary>
    /// Connection timeout in milliseconds. Default is 5000ms (5 seconds).
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Sync timeout in milliseconds for synchronous operations. Default is 5000ms.
    /// </summary>
    public int SyncTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Whether to abort connections on connect failure. Default is false.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Whether Redis backplane should be used (checks if connection string is provided and Enabled is true)
    /// </summary>
    public bool ShouldUseRedis => Enabled && !string.IsNullOrWhiteSpace(ConnectionString);
}
