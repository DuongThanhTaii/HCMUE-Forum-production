using UniHub.API.Middlewares;

namespace UniHub.API.Observability;

public sealed record UserActionLogSearchResult(
    IReadOnlyList<UserActionLogEntry> Items,
    long Total,
    int Page,
    int PageSize);
