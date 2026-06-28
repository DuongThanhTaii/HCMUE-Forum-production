using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Repository interface for managing users
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users
    /// </summary>
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search users by name or email (for messaging picker). Case-insensitive contains match.
    /// </summary>
    Task<IReadOnlyList<User>> SearchAsync(string searchTerm, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their unique ID
    /// </summary>
    Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already registered
    /// </summary>
    Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user
    /// </summary>
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
