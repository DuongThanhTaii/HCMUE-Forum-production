using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

/// <summary>
/// Command to assign a moderator to a course with permission checking.
/// </summary>
public sealed record AssignCourseModeratorCommand(
    Guid CourseId,
    Guid ModeratorId,
    Guid AssignedBy) : ICommand;
