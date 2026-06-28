namespace UniHub.Identity.Application.Authorization;

public sealed record PermissionOverrideItemResponse(
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
