namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record SetMaintenanceModeRequest(
    bool IsEnabled,
    string? Reason);
