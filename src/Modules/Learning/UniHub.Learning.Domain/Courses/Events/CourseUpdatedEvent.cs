using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi course được update
/// </summary>
public sealed record CourseUpdatedEvent(
    Guid CourseId,
    Guid UpdatedBy,
    DateTime OccurredOn) : IDomainEvent;
