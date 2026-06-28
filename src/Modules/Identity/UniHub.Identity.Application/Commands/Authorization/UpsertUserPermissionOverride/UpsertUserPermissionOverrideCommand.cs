using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;

public sealed record UpsertUserPermissionOverrideCommand(
    Guid UserId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue,
    string Effect,
    string? Reason,
    DateTime? ExpiresAtUtc) : ICommand;
