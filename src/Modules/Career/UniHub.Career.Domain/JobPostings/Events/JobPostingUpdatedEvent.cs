using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings.Events;

/// <summary>
/// Raised when a job posting's details are updated.
/// </summary>
public sealed record JobPostingUpdatedEvent(
    Guid JobPostingId,
    string Title,
    DateTime UpdatedAt) : IDomainEvent;
