using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.CourseManagement;

public sealed class DeleteCourseCommandHandler : ICommandHandler<DeleteCourseCommand>
{
    private readonly ICourseRepository _courseRepository;

    public DeleteCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        // Get course
        var course = await _courseRepository.GetByIdAsync(
            CourseId.Create(request.CourseId),
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error("Course.NotFound", "Course not found"));
        }

        // Delete course (soft delete)
        var result = course.Delete(request.DeletedBy);
        if (result.IsFailure)
        {
            return result;
        }

        // Persist changes
        await _courseRepository.UpdateAsync(course, cancellationToken);

        return Result.Success();
    }
}
