using UniHub.Forum.Application.Queries;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for Comment aggregate operations
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Gets a comment by its unique identifier
    /// </summary>
    Task<Comment?> GetByIdAsync(CommentId commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all comments for a specific post
    /// </summary>
    Task<IReadOnlyList<Comment>> GetByPostIdAsync(PostId postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new comment to the repository
    /// </summary>
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing comment
    /// </summary>
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment
    /// </summary>
    Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated comments for a specific post
    /// </summary>
    Task<Queries.GetComments.GetCommentsResult> GetCommentsByPostIdAsync(
        PostId postId,
        Guid? currentUserId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
