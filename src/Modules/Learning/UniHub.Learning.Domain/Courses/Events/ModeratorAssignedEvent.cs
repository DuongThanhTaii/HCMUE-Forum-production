using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi moderator được thêm vào course
/// </summary>
public sealed record ModeratorAssignedEvent(
    Guid CourseId,
    Guid ModeratorId,
    Guid AssignedBy,
    DateTime OccurredOn) : IDomainEvent;
