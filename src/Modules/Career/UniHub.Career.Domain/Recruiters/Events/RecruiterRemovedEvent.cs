using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters.Events;

public sealed record RecruiterRemovedEvent(
    RecruiterId RecruiterId,
    Guid RemovedBy) : IDomainEvent;
