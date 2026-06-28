namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Response after successful login
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
