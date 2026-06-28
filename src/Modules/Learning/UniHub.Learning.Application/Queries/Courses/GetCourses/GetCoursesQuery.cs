using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourses;

/// <summary>
/// Query courses with optional filtering and pagination.
/// </summary>
public sealed record GetCoursesQuery(
    Guid? FacultyId = null,
    string? Semester = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<PagedCourseListResponse>>;

/// <summary>
/// Paginated course list (same shape as other modules using page/pageSize).
/// </summary>
public sealed record PagedCourseListResponse(
    IReadOnlyList<CourseListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

/// <summary>
/// Response for course list item
/// </summary>
public sealed record CourseListItemResponse(
    Guid CourseId,
    string Code,
    string Name,
    string Description,
    string Semester,
    int Credits,
    Guid? FacultyId,
    DateTime CreatedAt,
    int DocumentCount
);
