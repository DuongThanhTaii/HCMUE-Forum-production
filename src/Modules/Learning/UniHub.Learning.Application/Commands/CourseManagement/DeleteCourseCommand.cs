using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.CourseManagement;

/// <summary>
/// Command to soft delete a course
/// </summary>
public sealed record DeleteCourseCommand(
    Guid CourseId,
    Guid DeletedBy) : ICommand;
