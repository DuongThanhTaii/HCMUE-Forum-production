using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

/// <summary>
/// Command to remove a moderator from a course with permission checking.
/// </summary>
public sealed record RemoveCourseModeratorCommand(
    Guid CourseId,
    Guid ModeratorId,
    Guid RemovedBy) : ICommand;
