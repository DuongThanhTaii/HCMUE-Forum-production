using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Repository interface for managing password reset tokens
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Gets a valid password reset token by token string
    /// </summary>
    Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new password reset token
    /// </summary>
    Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing password reset token
    /// </summary>
    Task UpdateAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all existing tokens for a user
    /// </summary>
    Task InvalidateUserTokensAsync(UserId userId, CancellationToken cancellationToken = default);
}
