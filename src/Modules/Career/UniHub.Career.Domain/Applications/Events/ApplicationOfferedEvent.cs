using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when a job offer is extended to a candidate.
/// </summary>
public sealed record ApplicationOfferedEvent(
    Guid ApplicationId,
    Guid OfferedBy,
    DateTime OfferedAt,
    string? OfferDetails) : IDomainEvent;
