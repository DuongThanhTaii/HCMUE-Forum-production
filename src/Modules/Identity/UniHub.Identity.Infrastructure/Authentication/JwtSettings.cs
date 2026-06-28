namespace UniHub.Identity.Infrastructure.Authentication;

/// <summary>
/// JWT authentication settings
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// The secret key used to sign JWT tokens
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// The issuer of the JWT tokens
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// The audience of the JWT tokens
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// Access token expiry duration in minutes (default: 15 minutes)
    /// </summary>
    public int AccessTokenExpiryMinutes { get; init; } = 15;

    /// <summary>
    /// Refresh token expiry duration in days (default: 7 days)
    /// </summary>
    public int RefreshTokenExpiryDays { get; init; } = 7;
}