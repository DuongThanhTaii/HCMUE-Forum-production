using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.FacultyManagement.CreateFaculty;

/// <summary>
/// Command to create a new faculty
/// </summary>
public sealed record CreateFacultyCommand(
    string Code,
    string Name,
    string? Description,
    Guid CreatedBy
) : IRequest<Result<Guid>>;
