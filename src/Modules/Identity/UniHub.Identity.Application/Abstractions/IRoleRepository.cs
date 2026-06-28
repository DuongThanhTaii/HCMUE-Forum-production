using UniHub.Identity.Domain.Roles;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Repository interface for managing roles
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by its unique ID
    /// </summary>
    Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken cancellationToken = default);

    Task<List<Role>> GetByIdsAsync(IEnumerable<RoleId> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its name
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles
    /// </summary>
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new role
    /// </summary>
    Task AddAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role
    /// </summary>
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role
    /// </summary>
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
}
