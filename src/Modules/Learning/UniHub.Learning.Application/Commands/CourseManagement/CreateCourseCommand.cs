using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.CourseManagement;

/// <summary>
/// Command to create a new course
/// </summary>
public sealed record CreateCourseCommand(
    string Code,
    string Name,
    string Description,
    string Semester,
    int Credits,
    Guid CreatedBy,
    Guid? FacultyId = null) : ICommand<Guid>;
