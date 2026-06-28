using UniHub.Forum.Domain.Posts;

namespace UniHub.Notification.Application.Abstractions;

/// <summary>
/// Cross-module port: resolves post author and title without depending on Forum's Application layer.
/// Implemented in UniHub.API or Forum.Infrastructure.
/// </summary>
public interface IPostAuthorLookup
{
    /// <summary>
    /// Returns (authorId, postTitle) for the given post, or null if not found.
    /// </summary>
    Task<(Guid AuthorId, string Title)?> GetAuthorAsync(PostId postId, CancellationToken ct = default);
}
