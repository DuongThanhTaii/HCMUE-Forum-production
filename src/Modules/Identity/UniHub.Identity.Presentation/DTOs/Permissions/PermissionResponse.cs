namespace UniHub.Identity.Presentation.DTOs.Permissions;

/// <summary>
/// Permission information
/// </summary>
public sealed record PermissionResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Module,
    string Resource,
    string Action);
