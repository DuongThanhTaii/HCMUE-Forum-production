namespace UniHub.API.Observability;

public sealed record UserActionLogQuery(
    string? ActorUserId,
    string? CorrelationId,
    string? TraceId,
    string? Method,
    string? PathContains,
    int? MinStatusCode,
    int? MaxStatusCode,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize,
    UserActionLogViewType ViewType);
