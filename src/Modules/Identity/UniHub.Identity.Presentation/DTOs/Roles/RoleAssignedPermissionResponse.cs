using System.Text.Json.Serialization;

namespace UniHub.Identity.Presentation.DTOs.Roles;

/// <summary>
/// One permission assignment on a role (global or scoped).
/// </summary>
public sealed record RoleAssignedPermissionResponse(
    [property: JsonPropertyName("permissionId")] Guid PermissionId,
    [property: JsonPropertyName("scopeType")] string ScopeType,
    [property: JsonPropertyName("scopeValue")] string? ScopeValue);
