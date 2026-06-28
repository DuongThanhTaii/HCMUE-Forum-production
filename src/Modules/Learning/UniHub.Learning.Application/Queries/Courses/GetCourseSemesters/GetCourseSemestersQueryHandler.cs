using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourseSemesters;

internal sealed class GetCourseSemestersQueryHandler
    : IRequestHandler<GetCourseSemestersQuery, Result<IReadOnlyList<string>>>
{
    private readonly ICourseRepository _courseRepository;

    public GetCourseSemestersQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetCourseSemestersQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _courseRepository.GetDistinctSemestersAsync(request.FacultyId, cancellationToken);
        return Result.Success<IReadOnlyList<string>>(list);
    }
}
