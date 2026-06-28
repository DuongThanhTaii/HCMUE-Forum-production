using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UniHub.API.Middlewares;

public sealed class UserActionLogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string ActionType { get; set; } = "UserAction";
    public string ActorUserId { get; set; } = "anonymous";
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "UnknownEndpoint";
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string RemoteIp { get; set; } = "unknown";
    public string UserAgent { get; set; } = "unknown";
    public string Scheme { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public string Result { get; set; } = "Success";
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }

    public string? RequestHeadersJson { get; set; }

    public string? RequestContentType { get; set; }

    public string? RequestBodyPreview { get; set; }

    public bool RequestBodyTruncated { get; set; }

    public string? ResponseHeadersJson { get; set; }

    public string? ResponseContentType { get; set; }

    public string? ResponseBodyPreview { get; set; }

    public bool ResponseBodyTruncated { get; set; }
}
