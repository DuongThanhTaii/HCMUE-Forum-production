using UniHub.Forum.Domain.Categories;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for managing categories
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Gets all categories
    /// </summary>
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its unique ID
    /// </summary>
    Task<Category?> GetByIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its slug
    /// </summary>
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a category exists
    /// </summary>
    Task<bool> ExistsAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new category
    /// </summary>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category
    /// </summary>
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
}
