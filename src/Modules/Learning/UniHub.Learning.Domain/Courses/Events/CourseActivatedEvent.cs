using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi course được activate
/// </summary>
public sealed record CourseActivatedEvent(
    Guid CourseId,
    Guid ActivatedBy,
    DateTime OccurredOn) : IDomainEvent;
