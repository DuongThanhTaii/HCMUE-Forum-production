namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Service to check if a user has moderator permissions for a course
/// </summary>
public interface IModeratorPermissionService
{
    /// <summary>
    /// Check if user is a moderator for the document's course
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user is a moderator, false otherwise</returns>
    Task<bool> IsModeratorForDocumentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is a moderator for a specific course
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="courseId">Course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user is a moderator, false otherwise</returns>
    Task<bool> IsModeratorForCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default);
}
