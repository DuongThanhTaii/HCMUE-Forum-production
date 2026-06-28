using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when an application status changes.
/// </summary>
public sealed record ApplicationStatusChangedEvent(
    Guid ApplicationId,
    ApplicationStatus OldStatus,
    ApplicationStatus NewStatus,
    Guid ChangedBy,
    DateTime ChangedAt,
    string? Reason) : IDomainEvent;
