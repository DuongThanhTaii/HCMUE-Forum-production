using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Authorization.RevokeUserPermissionOverride;

public sealed record RevokeUserPermissionOverrideCommand(
    Guid UserId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue) : ICommand;
