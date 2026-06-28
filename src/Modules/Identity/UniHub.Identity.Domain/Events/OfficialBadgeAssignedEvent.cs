using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Events;

public sealed record OfficialBadgeAssignedEvent(UserId UserId, OfficialBadge Badge) : IDomainEvent;