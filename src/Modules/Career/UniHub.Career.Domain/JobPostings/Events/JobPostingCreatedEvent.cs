using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings.Events;

/// <summary>
/// Raised when a new job posting is created.
/// </summary>
public sealed record JobPostingCreatedEvent(
    Guid JobPostingId,
    Guid CompanyId,
    string Title,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    string Location,
    DateTime CreatedAt) : IDomainEvent;
