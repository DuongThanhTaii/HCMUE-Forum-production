using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a refresh token
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;

/// <summary>
/// Response containing new tokens
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);
