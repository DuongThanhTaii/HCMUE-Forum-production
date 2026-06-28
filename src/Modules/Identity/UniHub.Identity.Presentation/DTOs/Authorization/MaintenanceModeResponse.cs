namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record MaintenanceModeResponse(
    bool IsEnabled,
    string? Reason,
    string UpdatedBy,
    DateTime UpdatedAtUtc,
    int Version);
