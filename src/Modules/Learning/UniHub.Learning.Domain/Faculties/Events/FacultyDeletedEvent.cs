using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Faculty bị xóa (soft delete)
/// </summary>
public sealed record FacultyDeletedEvent(
    Guid FacultyId,
    Guid DeletedBy,
    DateTime OccurredOn) : IDomainEvent;
