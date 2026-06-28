namespace UniHub.Learning.Presentation.DTOs.Courses;

public record CreateCourseRequest(
    string Code,
    string Name,
    string? Description,
    string Semester,
    int Credits,
    Guid CreatedBy,
    Guid? FacultyId);

public record CreateCourseResponse(
    Guid CourseId,
    string Code,
    string Name);

public record UpdateCourseRequest(
    string Name,
    string? Description,
    string Semester,
    int Credits);

public record DeleteCourseRequest(
    Guid DeletedBy);

public record AssignModeratorRequest(
    Guid ModeratorId,
    Guid AssignedBy);

public record RemoveModeratorRequest(
    Guid RemovedBy);
