using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourseById;

/// <summary>
/// Query to get course details by ID
/// </summary>
public sealed record GetCourseByIdQuery(
    Guid CourseId
) : IRequest<Result<CourseDetailResponse>>;

/// <summary>
/// Response containing course details
/// </summary>
public sealed record CourseDetailResponse(
    Guid CourseId,
    string Code,
    string Name,
    string Description,
    string Semester,
    int Credits,
    Guid? FacultyId,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<Guid> ModeratorIds,
    int DocumentCount,
    bool IsDeleted
);
