namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record PermissionOverrideResponse(
    Guid OverrideId,
    Guid PermissionId,
    string PermissionCode,
    string ScopeType,
    string? ScopeValue,
    string Effect,
    string? Reason,
    DateTime? ExpiresAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    bool IsRevoked);
