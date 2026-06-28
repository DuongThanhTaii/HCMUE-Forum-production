namespace UniHub.Identity.Application.Authorization;

public sealed record EndpointToggleItemResponse(
    string EndpointKey,
    bool IsEnabled,
    string? Reason,
    string UpdatedBy,
    DateTime UpdatedAtUtc,
    int Version);
