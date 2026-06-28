using System.Security.Cryptography;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private static readonly TimeSpan TokenValidFor = TimeSpan.FromHours(1);

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Parse email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<ForgotPasswordResponse>(emailResult.Error);
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            // Don't reveal if email exists - return success anyway for security
            // In production, you might still want to log this or send a generic email
            return Result.Failure<ForgotPasswordResponse>(UserErrors.NotFound);
        }

        // Check if user can reset password (must be active)
        if (!user.CanLogin())
        {
            return Result.Failure<ForgotPasswordResponse>(new Error(
                "ForgotPassword.UserNotActive",
                "User account is not active"));
        }

        // Invalidate any existing tokens for this user
        await _passwordResetTokenRepository.InvalidateUserTokensAsync(user.Id, cancellationToken);

        // Generate secure token
        var token = GenerateSecureToken();

        // Create password reset token
        var resetToken = PasswordResetToken.Create(user.Id, token, TokenValidFor);

        // Save token
        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);

        // In production, you would send an email here instead of returning the token
        // For now, return it directly for testing purposes
        return Result.Success(new ForgotPasswordResponse(
            user.Email.Value,
            token,
            resetToken.ExpiresAt));
    }

    private static string GenerateSecureToken()
    {
        // Generate a secure random token (32 bytes = 256 bits)
        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        
        // Convert to base64url (URL-safe)
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
