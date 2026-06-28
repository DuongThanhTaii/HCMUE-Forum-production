using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters.Events;

public sealed record RecruiterDeactivatedEvent(
    RecruiterId RecruiterId,
    Guid DeactivatedBy) : IDomainEvent;
