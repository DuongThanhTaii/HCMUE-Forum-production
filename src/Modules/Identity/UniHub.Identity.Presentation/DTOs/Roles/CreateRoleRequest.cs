namespace UniHub.Identity.Presentation.DTOs.Roles;

/// <summary>
/// Request to create a new role
/// </summary>
public sealed record CreateRoleRequest(
    string Name,
    string? Description);
