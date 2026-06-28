using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

public sealed class RemoveCourseModeratorCommandHandler : ICommandHandler<RemoveCourseModeratorCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IModeratorManagementPermissionService _permissionService;

    public RemoveCourseModeratorCommandHandler(
        ICourseRepository courseRepository,
        IModeratorManagementPermissionService permissionService)
    {
        _courseRepository = courseRepository;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(RemoveCourseModeratorCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        var canManage = await _permissionService.CanManageCourseModeratorsAsync(
            request.RemovedBy, request.CourseId, cancellationToken);

        if (!canManage)
        {
            return Result.Failure(new Error(
                "Course.Unauthorized",
                "User is not authorized to manage moderators for this course"));
        }

        // Get course
        var course = await _courseRepository.GetByIdAsync(
            CourseId.Create(request.CourseId), cancellationToken);

        if (course is null)
        {
            return Result.Failure(new Error(
                "Course.NotFound",
                $"Course with ID '{request.CourseId}' was not found"));
        }

        // Remove moderator
        var result = course.RemoveModerator(request.ModeratorId, request.RemovedBy);
        if (result.IsFailure)
        {
            return result;
        }

        // Save
        await _courseRepository.UpdateAsync(course, cancellationToken);

        return Result.Success();
    }
}
