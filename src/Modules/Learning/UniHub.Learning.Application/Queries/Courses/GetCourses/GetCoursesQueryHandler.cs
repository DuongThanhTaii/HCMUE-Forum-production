using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourses;

/// <summary>
/// Handler for GetCoursesQuery
/// </summary>
internal sealed class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, Result<PagedCourseListResponse>>
{
    private readonly ICourseRepository _courseRepository;

    public GetCoursesQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result<PagedCourseListResponse>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (courses, totalCount) = await _courseRepository.SearchPagedAsync(
            request.FacultyId,
            request.Semester,
            request.SearchTerm,
            page,
            pageSize,
            cancellationToken);

        var responses = courses
            .Select(course => new CourseListItemResponse(
                course.Id.Value,
                course.Code,
                course.Name,
                course.Description,
                course.Semester,
                course.Credits,
                course.FacultyId,
                course.CreatedAt,
                course.DocumentCount))
            .ToList();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result.Success(new PagedCourseListResponse(
            responses,
            page,
            pageSize,
            totalCount,
            totalPages));
    }
}
