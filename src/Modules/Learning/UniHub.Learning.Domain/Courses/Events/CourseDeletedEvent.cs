using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi course bị xóa
/// </summary>
public sealed record CourseDeletedEvent(
    Guid CourseId,
    Guid DeletedBy,
    DateTime OccurredOn) : IDomainEvent;
