using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Faculties;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Faculties.GetFacultyById;

/// <summary>
/// Handler for GetFacultyByIdQuery.
/// </summary>
internal sealed class GetFacultyByIdQueryHandler
    : IRequestHandler<GetFacultyByIdQuery, Result<FacultyDetailResponse>>
{
    private readonly IFacultyRepository _facultyRepository;

    public GetFacultyByIdQueryHandler(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<FacultyDetailResponse>> Handle(
        GetFacultyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var facultyId = FacultyId.Create(request.FacultyId);
        var faculty = await _facultyRepository.GetByIdAsync(facultyId, cancellationToken);

        if (faculty is null)
        {
            return Result.Failure<FacultyDetailResponse>(
                new Error("Faculty.NotFound", $"Faculty with ID {request.FacultyId} not found"));
        }

        var response = new FacultyDetailResponse(
            faculty.Id.Value,
            faculty.Code.Value,
            faculty.Name.Value,
            faculty.Description.Value,
            faculty.Status.ToString(),
            faculty.ManagerId,
            faculty.CourseCount,
            faculty.CreatedBy,
            faculty.CreatedAt,
            faculty.UpdatedAt);

        return Result.Success(response);
    }
}
