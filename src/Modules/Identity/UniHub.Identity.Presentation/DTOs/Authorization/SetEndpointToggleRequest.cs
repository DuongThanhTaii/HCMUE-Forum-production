namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record SetEndpointToggleRequest(
    bool IsEnabled,
    string? Reason);
