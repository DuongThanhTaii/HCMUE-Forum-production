using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Events;

public sealed record RoleAssignedEvent(UserId UserId, RoleId RoleId) : IDomainEvent;