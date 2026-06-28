using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Faculty được activate lại
/// </summary>
public sealed record FacultyActivatedEvent(
    Guid FacultyId,
    Guid ActivatedBy,
    DateTime OccurredOn) : IDomainEvent;
