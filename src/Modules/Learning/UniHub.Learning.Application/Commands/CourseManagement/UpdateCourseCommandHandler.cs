using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class UpdateCourseCommandHandler : ICommandHandler<UpdateCourseCommand>
{
    private readonly ICourseRepository _courseRepository;

    public UpdateCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        // Get course
        var course = await _courseRepository.GetByIdAsync(
            CourseId.Create(request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course not found"));
        }

        // Create value objects
        var nameResult = CourseName.Create(request.Name);
        if (nameResult.IsFailure)
        {
            return Result.Failure(nameResult.Error);
        }

        var descriptionResult = CourseDescription.Create(request.Description);
        if (descriptionResult.IsFailure)
        {
            return Result.Failure(descriptionResult.Error);
        }

        var semesterResult = Semester.Create(request.Semester);
        if (semesterResult.IsFailure)
        {
            return Result.Failure(semesterResult.Error);
        }

        // Update course
        var result = course.Update(
            nameResult.Value,
            descriptionResult.Value,
            semesterResult.Value,
            request.Credits,
            course.FacultyId);

        if (result.IsFailure)
        {
            return result;
        }

        // Persist changes
        await _courseRepository.UpdateAsync(course, cancellationToken);

        return Result.Success();
    }
}
