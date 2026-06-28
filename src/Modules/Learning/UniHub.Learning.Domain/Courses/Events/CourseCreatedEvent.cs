using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Courses.Events;

/// <summary>
/// Domain event khi course được tạo
/// </summary>
public sealed record CourseCreatedEvent(
    Guid CourseId,
    string CourseCode,
    string CourseName,
    Guid CreatedBy,
    DateTime OccurredOn) : IDomainEvent;
