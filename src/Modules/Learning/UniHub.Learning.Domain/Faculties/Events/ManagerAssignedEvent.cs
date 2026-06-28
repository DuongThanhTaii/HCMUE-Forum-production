using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Manager được assign cho Faculty
/// </summary>
public sealed record ManagerAssignedEvent(
    Guid FacultyId,
    Guid ManagerId,
    Guid AssignedBy,
    DateTime OccurredOn) : IDomainEvent;
