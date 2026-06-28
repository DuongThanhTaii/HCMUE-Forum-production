using MediatR;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.RevokeRefreshToken;

/// <summary>
/// Handler for RevokeRefreshTokenCommand
/// </summary>
internal sealed class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokeRefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result> Handle(
        RevokeRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var userId = UserId.Create(command.UserId);

        // Revoke all active tokens for the user
        await _refreshTokenRepository.RevokeAllByUserIdAsync(userId, command.IpAddress, cancellationToken);

        return Result.Success();
    }
}
