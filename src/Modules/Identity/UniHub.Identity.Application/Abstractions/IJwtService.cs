using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// JWT token service interface for generating and validating JWT tokens
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT access token for the specified user
    /// </summary>
    /// <param name="user">The user to generate token for</param>
    /// <param name="roleNames">Optional role names to include in claims</param>
    /// <returns>JWT token string</returns>
    Result<string> GenerateAccessToken(User user, IEnumerable<string>? roleNames = null);

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>User ID if token is valid</returns>
    Result<UserId> ValidateToken(string token);

    /// <summary>
    /// Generates a refresh token for the specified user
    /// </summary>
    /// <param name="userId">The user ID to generate refresh token for</param>
    /// <param name="ipAddress">Client IP address for security tracking</param>
    /// <returns>RefreshToken entity</returns>
    RefreshToken GenerateRefreshToken(UserId userId, string? ipAddress = null);

    /// <summary>
    /// Gets the expiration time for access tokens (15 minutes)
    /// </summary>
    TimeSpan AccessTokenExpiry { get; }

    /// <summary>
    /// Gets the expiration time for refresh tokens (7 days)
    /// </summary>
    TimeSpan RefreshTokenExpiry { get; }
}