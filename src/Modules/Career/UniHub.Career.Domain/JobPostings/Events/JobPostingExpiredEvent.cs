using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings.Events;

/// <summary>
/// Raised when a job posting has expired (deadline passed).
/// </summary>
public sealed record JobPostingExpiredEvent(
    Guid JobPostingId,
    Guid CompanyId,
    string Title,
    DateTime Deadline,
    DateTime ExpiredAt) : IDomainEvent;
