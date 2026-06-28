using UniHub.Forum.Application.Queries.GetPopularTags;
using UniHub.Forum.Application.Queries.GetTags;
using UniHub.Forum.Domain.Tags;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for managing tags
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Gets a tag by ID
    /// </summary>
    Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tag by name
    /// </summary>
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tag by slug
    /// </summary>
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated tags with optional search and sorting
    /// </summary>
    Task<GetTagsResult> GetTagsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool orderByUsage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets most popular tags
    /// </summary>
    Task<IEnumerable<PopularTagDto>> GetPopularTagsAsync(
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new tag
    /// </summary>
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tag
    /// </summary>
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tag
    /// </summary>
    Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);
}
