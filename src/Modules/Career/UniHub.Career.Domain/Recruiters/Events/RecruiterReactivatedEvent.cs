using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters.Events;

public sealed record RecruiterReactivatedEvent(
    RecruiterId RecruiterId,
    Guid ReactivatedBy) : IDomainEvent;
