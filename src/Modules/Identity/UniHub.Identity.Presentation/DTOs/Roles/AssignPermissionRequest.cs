namespace UniHub.Identity.Presentation.DTOs.Roles;

/// <summary>
/// Request to assign permission to role
/// </summary>
public sealed record AssignPermissionRequest(
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue = null);
