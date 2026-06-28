using UniHub.Learning.Domain.Faculties;

namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Repository interface for Faculty aggregate
/// </summary>
public interface IFacultyRepository
{
    /// <summary>
    /// Add a new faculty
    /// </summary>
    Task AddAsync(Faculty faculty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing faculty
    /// </summary>
    Task UpdateAsync(Faculty faculty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty by ID
    /// </summary>
    Task<Faculty?> GetByIdAsync(FacultyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all faculties
    /// </summary>
    Task<IReadOnlyList<Faculty>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if faculty code already exists
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if faculty exists
    /// </summary>
    Task<bool> ExistsAsync(FacultyId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get faculty by code
    /// </summary>
    Task<Faculty?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
