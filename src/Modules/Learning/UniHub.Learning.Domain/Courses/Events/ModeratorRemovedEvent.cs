using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi moderator bị remove khỏi course
/// </summary>
public sealed record ModeratorRemovedEvent(
    Guid CourseId,
    Guid ModeratorId,
    Guid RemovedBy,
    DateTime OccurredOn) : IDomainEvent;
