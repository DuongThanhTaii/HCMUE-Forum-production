using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Faculties.GetFaculties;

/// <summary>
/// Query to get all faculties
/// </summary>
public sealed record GetFacultiesQuery() : IRequest<Result<List<FacultyListItemResponse>>>;

/// <summary>
/// Response for faculty list item
/// </summary>
public sealed record FacultyListItemResponse(
    Guid FacultyId,
    string Code,
    string Name,
    string Description,
    string Status,
    int CourseCount,
    DateTime CreatedAt
);
