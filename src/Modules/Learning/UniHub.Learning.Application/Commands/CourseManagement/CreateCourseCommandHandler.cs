using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;

    public CreateCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // Check if course code already exists
        var codeExists = await _courseRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<Guid>(new Error("Course.CodeAlreadyExists", 
                $"Course with code '{request.Code}' already exists"));
        }

        // Create value objects
        var codeResult = CourseCode.Create(request.Code);
        if (codeResult.IsFailure)
        {
            return Result.Failure<Guid>(codeResult.Error);
        }

        var nameResult = CourseName.Create(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<Guid>(nameResult.Error);
        }

        var descriptionResult = CourseDescription.Create(request.Description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure<Guid>(descriptionResult.Error);
        }

        var semesterResult = Semester.Create(request.Semester);
        if (semesterResult.IsFailure)
        {
            return Result.Failure<Guid>(semesterResult.Error);
        }

        // Create course aggregate
        var courseResult = Course.Create(
            codeResult.Value,
            nameResult.Value,
            descriptionResult.Value,
            semesterResult.Value,
            request.Credits,
            request.CreatedBy,
            request.FacultyId);

        if (courseResult.IsFailure)
        {
            return Result.Failure<Guid>(courseResult.Error);
        }

        // Persist course
        await _courseRepository.AddAsync(courseResult.Value, cancellationToken);

        return Result.Success(courseResult.Value.Id.Value);
    }
}
