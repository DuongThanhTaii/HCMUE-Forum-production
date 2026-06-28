namespace UniHub.API.Middlewares;

public sealed class UserActionLoggingOptions
{
    public const string SectionName = "Observability:UserActionLogging";

    public bool Enabled { get; set; } = true;
    public bool PersistToMongo { get; set; } = true;
    public string MongoCollectionName { get; set; } = "user_action_logs";
    public int RetentionDays { get; set; } = 90;
    public int DefaultQueryPageSize { get; set; } = 100;
    public int MaxQueryPageSize { get; set; } = 500;
    public string CorrelationHeaderName { get; set; } = "X-Correlation-Id";
    public string[] ExcludedPathPrefixes { get; set; } = ["/health", "/openapi", "/scalar", "/hubs"];

    /// <summary>When true, captures redacted headers and bounded UTF-8 body previews for Mongo + admin UI.</summary>
    public bool CaptureHttpDetails { get; set; } = true;

    /// <summary>Max bytes read from request/response for stored preview text (each).</summary>
    public int MaxCapturedBodyBytes { get; set; } = 16384;

    /// <summary>Max JSON length for stored header maps.</summary>
    public int MaxCapturedHeadersJsonChars { get; set; } = 8192;
}
