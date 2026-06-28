using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourseById;

/// <summary>
/// Handler for GetCourseByIdQuery
/// </summary>
internal sealed class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseDetailResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IDocumentRepository _documentRepository;

    public GetCourseByIdQueryHandler(
        ICourseRepository courseRepository,
        IDocumentRepository documentRepository)
    {
        _courseRepository = courseRepository;
        _documentRepository = documentRepository;
    }

    public async Task<Result<CourseDetailResponse>> Handle(
        GetCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var courseId = CourseId.Create(request.CourseId);
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);

        if (course is null)
        {
            return Result.Failure<CourseDetailResponse>(
                new Error("Course.NotFound", $"Course with ID {request.CourseId} not found"));
        }

        // Get document count
        var documents = await _documentRepository.GetByCourseIdAsync(request.CourseId, cancellationToken);

        var response = new CourseDetailResponse(
            course.Id.Value,
            course.Code,
            course.Name,
            course.Description,
            course.Semester,
            course.Credits,
            course.FacultyId,
            course.CreatedBy,
            course.CreatedAt,
            course.UpdatedAt,
            course.ModeratorIds.ToList(),
            documents.Count,
            course.Status == Domain.Courses.CourseStatus.Deleted
        );

        return Result.Success(response);
    }
}
