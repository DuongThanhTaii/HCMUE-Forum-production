using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Courses.GetCourseSemesters;

public sealed record GetCourseSemestersQuery(Guid? FacultyId = null)
    : IRequest<Result<IReadOnlyList<string>>>;
