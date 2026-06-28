namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record RevokePermissionOverrideRequest(
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue);
