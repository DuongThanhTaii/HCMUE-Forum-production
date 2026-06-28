using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi course được archive
/// </summary>
public sealed record CourseArchivedEvent(
    Guid CourseId,
    Guid ArchivedBy,
    DateTime OccurredOn) : IDomainEvent;
