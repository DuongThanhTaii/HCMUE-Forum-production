using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Repository interface for managing permissions
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Gets a permission by its ID
    /// </summary>
    Task<Permission?> GetByIdAsync(PermissionId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by its code
    /// </summary>
    Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions
    /// </summary>
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
}
