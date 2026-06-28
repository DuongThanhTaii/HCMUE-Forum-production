namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record UpsertPermissionOverrideRequest(
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue,
    string Effect,
    string? Reason,
    DateTime? ExpiresAtUtc);
