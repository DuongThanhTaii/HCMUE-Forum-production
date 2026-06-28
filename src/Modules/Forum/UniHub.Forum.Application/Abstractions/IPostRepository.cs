using UniHub.Forum.Application.Queries.SearchPosts;
using UniHub.Forum.Application.Queries;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for managing posts
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Gets all posts
    /// </summary>
    Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a post by its unique ID
    /// </summary>
    Task<Post?> GetByIdAsync(PostId postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a post by its slug
    /// </summary>
    Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slug is already in use
    /// </summary>
    Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new post
    /// </summary>
    Task AddAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing post
    /// </summary>
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post
    /// </summary>
    Task DeleteAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches posts using full-text search
    /// </summary>
    Task<SearchPostsResult> SearchAsync(
        string searchTerm,
        Guid? categoryId = null,
        int? postType = null,
        IEnumerable<string>? tags = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of posts with optional filtering.
    /// sortBy: 0 = newest, 1 = top (VoteScore desc)
    /// </summary>
    Task<Queries.GetPosts.GetPostsResult> GetPostsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        Guid? categoryId = null,
        Guid? threadChannelId = null,
        int? type = null,
        int? status = null,
        int sortBy = 0,
        IReadOnlyList<Guid>? categoryIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information about a post by ID
    /// </summary>
    Task<Queries.GetPostById.PostDetailResult?> GetPostDetailsAsync(
        PostId postId,
        CancellationToken cancellationToken = default);
}
