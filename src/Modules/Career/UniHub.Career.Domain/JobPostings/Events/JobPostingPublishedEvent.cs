using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.JobPostings.Events;

/// <summary>
/// Raised when a job posting is published and becomes visible.
/// </summary>
public sealed record JobPostingPublishedEvent(
    Guid JobPostingId,
    Guid CompanyId,
    string Title,
    DateTime PublishedAt) : IDomainEvent;
