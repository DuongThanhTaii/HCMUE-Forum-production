namespace UniHub.Identity.Application.Commands.Login;

/// <summary>
/// Response containing authentication tokens
/// </summary>
public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
