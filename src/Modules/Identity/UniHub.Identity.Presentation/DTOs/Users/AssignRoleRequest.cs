namespace UniHub.Identity.Presentation.DTOs.Users;

/// <summary>
/// Request to assign role to user
/// </summary>
public sealed record AssignRoleRequest(
    Guid RoleId);
