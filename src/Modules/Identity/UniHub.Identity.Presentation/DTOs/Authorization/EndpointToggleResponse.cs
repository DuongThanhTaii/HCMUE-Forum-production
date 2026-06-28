namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record EndpointToggleResponse(
    string EndpointKey,
    bool IsEnabled,
    string? Reason,
    string UpdatedBy,
    DateTime UpdatedAtUtc,
    int Version);
