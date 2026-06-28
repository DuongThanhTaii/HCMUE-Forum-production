namespace UniHub.Notification.Application.Abstractions;

/// <summary>
/// Cross-module lookups for notification recipients and display context.
/// </summary>
public interface INotificationRecipientResolver
{
    Task<(Guid AuthorId, string Title)?> GetPostAuthorAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<(Guid AuthorId, Guid? CategoryId, string Title)?> GetPostContextAsync(
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<(Guid AuthorId, Guid PostId, string PostTitle)?> GetCommentContextAsync(
        Guid commentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forum moderators for a category; falls back to global moderators/admins when category is null.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetForumModeratorUserIdsAsync(
        Guid? categoryId,
        CancellationToken cancellationToken = default);

    Task<(Guid UploaderId, string Title, Guid? CourseId)?> GetDocumentContextAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetLearningModeratorUserIdsAsync(
        Guid? courseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetConversationParticipantIdsExceptAsync(
        Guid conversationId,
        Guid exceptUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAdminAndModeratorUserIdsAsync(CancellationToken cancellationToken = default);
}
