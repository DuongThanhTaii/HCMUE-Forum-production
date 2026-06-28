namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Request to refresh access token
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken);
