using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Events;

public sealed record UserStatusChangedEvent(UserId UserId, UserStatus OldStatus, UserStatus NewStatus) : IDomainEvent;