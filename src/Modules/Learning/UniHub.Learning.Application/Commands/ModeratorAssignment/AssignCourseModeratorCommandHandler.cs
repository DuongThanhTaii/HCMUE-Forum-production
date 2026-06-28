using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Courses;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.ModeratorAssignment;

public sealed class AssignCourseModeratorCommandHandler : ICommandHandler<AssignCourseModeratorCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IModeratorManagementPermissionService _permissionService;

    public AssignCourseModeratorCommandHandler(
        ICourseRepository courseRepository,
        IModeratorManagementPermissionService permissionService)
    {
        _courseRepository = courseRepository;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(AssignCourseModeratorCommand request, CancellationToken cancellationToken)
    {
        // Check permission
        var canManage = await _permissionService.CanManageCourseModeratorsAsync(
            request.AssignedBy, request.CourseId, cancellationToken);

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

        // Assign moderator
        var result = course.AssignModerator(request.ModeratorId, request.AssignedBy);
        if (result.IsFailure)
        {
            return result;
        }

        // Save
        await _courseRepository.UpdateAsync(course, cancellationToken);

        return Result.Success();
    }
}
