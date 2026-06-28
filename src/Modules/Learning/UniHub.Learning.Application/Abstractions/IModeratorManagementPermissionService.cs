namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Service for checking moderator management permissions.
/// </summary>
public interface IModeratorManagementPermissionService
{
    /// <summary>
    /// Checks if a user can manage moderators for a specific course.
    /// This typically requires admin or course manager role.
    /// </summary>
    Task<bool> CanManageCourseModeratorsAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
