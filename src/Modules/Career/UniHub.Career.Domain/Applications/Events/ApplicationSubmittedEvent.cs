using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when a job application is submitted.
/// </summary>
public sealed record ApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid JobPostingId,
    Guid ApplicantId,
    DateTime SubmittedAt,
    bool HasCoverLetter,
    bool HasResume) : IDomainEvent;
