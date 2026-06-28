using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Faculties.Events;

/// <summary>
/// Domain event khi Faculty được tạo
/// </summary>
public sealed record FacultyCreatedEvent(
    Guid FacultyId,
    string FacultyCode,
    string FacultyName,
    Guid CreatedBy,
    DateTime OccurredOn) : IDomainEvent;
