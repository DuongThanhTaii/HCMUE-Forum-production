using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Faculty bị deactivate
/// </summary>
public sealed record FacultyDeactivatedEvent(
    Guid FacultyId,
    Guid DeactivatedBy,
    DateTime OccurredOn) : IDomainEvent;
