using MediatR;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand
/// </summary>
internal sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        // Get refresh token from database
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null)
        {
            return Result.Failure<RefreshTokenResponse>(
                new Error("RefreshToken.Invalid", "Invalid refresh token"));
        }

        // Check if token is still active
        if (!refreshToken.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(
                new Error("RefreshToken.Inactive", "Refresh token is no longer active"));
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(
                new Error("User.NotFound", "User not found"));
        }

        // Generate new tokens
        var accessTokenResult = _jwtService.GenerateAccessToken(user);
        if (accessTokenResult.IsFailure)
        {
            return Result.Failure<RefreshTokenResponse>(accessTokenResult.Error);
        }

        var newAccessToken = accessTokenResult.Value;
        var accessTokenExpiresAt = DateTime.UtcNow.Add(_jwtService.AccessTokenExpiry);
        
        // Generate new refresh token
        var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id, null);

        // Revoke old token
        refreshToken.Revoke(null);

        // Save changes
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        var refreshTokenExpiresAt = DateTime.UtcNow.Add(_jwtService.RefreshTokenExpiry);

        var response = new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken.Token,
            accessTokenExpiresAt,
            refreshTokenExpiresAt
        );

        return Result.Success(response);
    }
}
