using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Faculty được update
/// </summary>
public sealed record FacultyUpdatedEvent(
    Guid FacultyId,
    Guid UpdatedBy,
    DateTime OccurredOn) : IDomainEvent;
