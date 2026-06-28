using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.GetUserPermissions;

/// <summary>
/// Query to get all permissions for a user
/// </summary>
public sealed record GetUserPermissionsQuery(Guid UserId) : IQuery<UserPermissionsResponse>;
