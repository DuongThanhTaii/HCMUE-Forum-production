using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Authorization.UpsertGroupPermissionOverride;

public sealed record UpsertGroupPermissionOverrideCommand(
    Guid GroupId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue,
    string Effect,
    string? Reason,
    DateTime? ExpiresAtUtc) : ICommand;
