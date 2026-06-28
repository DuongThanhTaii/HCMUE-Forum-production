namespace UniHub.Learning.Presentation.DTOs.Faculties;

public record CreateFacultyRequest(
    string Code,
    string Name,
    string? Description,
    Guid? ManagerId);

public record CreateFacultyResponse(
    Guid FacultyId,
    string Code,
    string Name);
