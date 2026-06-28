using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Applications.Events;

/// <summary>
/// Domain event raised when an application is rejected.
/// </summary>
public sealed record ApplicationRejectedEvent(
    Guid ApplicationId,
    Guid RejectedBy,
    DateTime RejectedAt,
    string? Reason) : IDomainEvent;
