using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Authorization.RevokeGroupPermissionOverride;

public sealed record RevokeGroupPermissionOverrideCommand(
    Guid GroupId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue) : ICommand;
