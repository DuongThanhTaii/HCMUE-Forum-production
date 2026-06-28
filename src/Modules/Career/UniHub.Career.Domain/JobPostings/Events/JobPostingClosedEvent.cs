using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings.Events;

/// <summary>
/// Raised when a job posting is closed.
/// </summary>
public sealed record JobPostingClosedEvent(
    Guid JobPostingId,
    Guid CompanyId,
    string Title,
    string Reason,
    DateTime ClosedAt) : IDomainEvent;
