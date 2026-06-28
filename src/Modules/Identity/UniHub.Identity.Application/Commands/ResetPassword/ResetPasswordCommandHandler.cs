using UniHub.Identity.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.ResetPassword;

internal sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Get and validate token
        var resetToken = await _passwordResetTokenRepository.GetValidTokenAsync(
            request.Token, 
            cancellationToken);

        if (resetToken is null || !resetToken.IsValid())
        {
            return Result.Failure(new Error(
                "ResetPassword.InvalidToken",
                "Invalid or expired reset token"));
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(new Error(
                "ResetPassword.UserNotFound",
                "User not found"));
        }

        // Hash new password
        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        // Change password
        var changeResult = user.ChangePassword(newPasswordHash);
        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        // Mark token as used
        resetToken.MarkAsUsed();

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _passwordResetTokenRepository.UpdateAsync(resetToken, cancellationToken);

        return Result.Success();
    }
}
