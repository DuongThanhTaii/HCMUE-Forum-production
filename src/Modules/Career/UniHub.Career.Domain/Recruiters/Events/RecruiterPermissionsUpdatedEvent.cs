using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters.Events;

public sealed record RecruiterPermissionsUpdatedEvent(
    RecruiterId RecruiterId,
    Guid UpdatedBy,
    RecruiterPermissions NewPermissions) : IDomainEvent;
