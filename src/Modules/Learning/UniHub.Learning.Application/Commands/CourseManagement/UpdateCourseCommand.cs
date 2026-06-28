using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.CourseManagement;

/// <summary>
/// Command to update course information
/// </summary>
public sealed record UpdateCourseCommand(
    Guid CourseId,
    string Name,
    string Description,
    string Semester,
    int Credits) : ICommand;
