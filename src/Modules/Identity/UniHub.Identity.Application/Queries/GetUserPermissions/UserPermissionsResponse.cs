namespace UniHub.Identity.Application.Queries.GetUserPermissions;

/// <summary>
/// Response containing user permissions
/// </summary>
public sealed record UserPermissionsResponse(
    Guid UserId,
    IReadOnlyList<PermissionDto> Permissions);

/// <summary>
/// Permission DTO
/// </summary>
public sealed record PermissionDto(
    Guid PermissionId,
    string Code,
    string Name,
    string? Description,
    string Module,
    PermissionScopeDto? Scope);

/// <summary>
/// Permission scope DTO
/// </summary>
public sealed record PermissionScopeDto(
    string Type,
    string? Value);
