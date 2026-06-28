using UniHub.Forum.Application.Queries.GetBookmarkedPosts;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for managing bookmarks
/// </summary>
public interface IBookmarkRepository
{
    /// <summary>
    /// Gets a bookmark by user and post
    /// </summary>
    Task<Bookmark?> GetByUserAndPostAsync(
        Guid userId,
        PostId postId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated bookmarked posts for a user
    /// </summary>
    Task<GetBookmarkedPostsResult> GetBookmarkedPostsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new bookmark
    /// </summary>
    Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a bookmark
    /// </summary>
    Task RemoveAsync(Bookmark bookmark, CancellationToken cancellationToken = default);
}
