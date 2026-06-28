using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when a candidate withdraws their application.
/// </summary>
public sealed record ApplicationWithdrawnEvent(
    Guid ApplicationId,
    Guid ApplicantId,
    DateTime WithdrawnAt,
    string? Reason) : IDomainEvent;
