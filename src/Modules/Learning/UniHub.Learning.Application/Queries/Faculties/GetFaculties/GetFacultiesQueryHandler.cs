using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Faculties.GetFaculties;

/// <summary>
/// Handler for GetFacultiesQuery
/// </summary>
internal sealed class GetFacultiesQueryHandler : IRequestHandler<GetFacultiesQuery, Result<List<FacultyListItemResponse>>>
{
    private readonly IFacultyRepository _facultyRepository;

    public GetFacultiesQueryHandler(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<List<FacultyListItemResponse>>> Handle(
        GetFacultiesQuery request,
        CancellationToken cancellationToken)
    {
        var faculties = await _facultyRepository.GetAllAsync(cancellationToken);

        var responses = faculties.Select(f => new FacultyListItemResponse(
            f.Id.Value,
            f.Code.Value,
            f.Name.Value,
            f.Description.Value,
            f.Status.ToString(),
            f.CourseCount,
            f.CreatedAt
        )).ToList();

        return Result.Success(responses);
    }
}
