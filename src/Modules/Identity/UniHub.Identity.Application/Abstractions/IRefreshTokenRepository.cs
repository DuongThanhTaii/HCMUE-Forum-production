using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Repository interface for managing refresh tokens
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its token string
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active refresh tokens for a user
    /// </summary>
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new refresh token
    /// </summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing refresh token
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user
    /// </summary>
    Task RevokeAllByUserIdAsync(UserId userId, string? revokedByIp = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens (cleanup task)
    /// </summary>
    Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}
