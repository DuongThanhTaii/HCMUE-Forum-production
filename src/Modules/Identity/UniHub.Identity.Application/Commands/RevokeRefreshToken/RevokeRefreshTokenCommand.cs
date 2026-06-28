using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.RevokeRefreshToken;

/// <summary>
/// Command to revoke/logout a user's refresh tokens
/// </summary>
public sealed record RevokeRefreshTokenCommand(
    Guid UserId,
    string? IpAddress = null
) : IRequest<Result>;
