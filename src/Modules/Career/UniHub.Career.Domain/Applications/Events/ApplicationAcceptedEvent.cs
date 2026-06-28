using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when a candidate accepts a job offer.
/// </summary>
public sealed record ApplicationAcceptedEvent(
    Guid ApplicationId,
    Guid ApplicantId,
    DateTime AcceptedAt) : IDomainEvent;
