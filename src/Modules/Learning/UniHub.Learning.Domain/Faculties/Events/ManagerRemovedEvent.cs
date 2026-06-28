using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Manager được remove khỏi Faculty
/// </summary>
public sealed record ManagerRemovedEvent(
    Guid FacultyId,
    Guid ManagerId,
    Guid RemovedBy,
    DateTime OccurredOn) : IDomainEvent;
