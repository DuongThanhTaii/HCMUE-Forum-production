namespace UniHub.Identity.Presentation.DTOs.Roles;

/// <summary>
/// Request to update a role
/// </summary>
public sealed record UpdateRoleRequest(
    string Name,
    string? Description);
