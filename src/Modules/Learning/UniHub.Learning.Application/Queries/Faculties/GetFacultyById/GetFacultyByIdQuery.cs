using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Faculties.GetFacultyById;

/// <summary>
/// Query to get a faculty by its ID.
/// </summary>
public sealed record GetFacultyByIdQuery(Guid FacultyId) : IRequest<Result<FacultyDetailResponse>>;

/// <summary>
/// Detailed response for a single faculty.
/// </summary>
public sealed record FacultyDetailResponse(
    Guid FacultyId,
    string Code,
    string Name,
    string Description,
    string Status,
    Guid? ManagerId,
    int CourseCount,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
