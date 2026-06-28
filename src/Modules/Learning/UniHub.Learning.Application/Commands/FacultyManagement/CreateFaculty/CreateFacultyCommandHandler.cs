using MediatR;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Faculties.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.FacultyManagement.CreateFaculty;

/// <summary>
/// Handler for CreateFacultyCommand
/// </summary>
internal sealed class CreateFacultyCommandHandler : IRequestHandler<CreateFacultyCommand, Result<Guid>>
{
    private readonly IFacultyRepository _facultyRepository;

    public CreateFacultyCommandHandler(
        IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<Guid>> Handle(
        CreateFacultyCommand command,
        CancellationToken cancellationToken)
    {
        // Check if code already exists
        var codeExists = await _facultyRepository.ExistsByCodeAsync(command.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<Guid>(
                new Error("Faculty.CodeAlreadyExists", $"Faculty with code {command.Code} already exists"));
        }

        // Create value objects
        var codeResult = FacultyCode.Create(command.Code);
        if (codeResult.IsFailure)
        {
            return Result.Failure<Guid>(codeResult.Error);
        }

        var nameResult = FacultyName.Create(command.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Guid>(nameResult.Error);
        }

        var descriptionResult = FacultyDescription.Create(command.Description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure<Guid>(descriptionResult.Error);
        }

        // Create faculty
        var facultyResult = Domain.Faculties.Faculty.Create(
            codeResult.Value,
            nameResult.Value,
            descriptionResult.Value,
            command.CreatedBy);

        if (facultyResult.IsFailure)
        {
            return Result.Failure<Guid>(facultyResult.Error);
        }

        await _facultyRepository.AddAsync(facultyResult.Value, cancellationToken);

        return Result.Success(facultyResult.Value.Id.Value);
    }
}
