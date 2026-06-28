using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.ForgotPassword;

/// <summary>
/// Command to initiate password reset process
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand<ForgotPasswordResponse>;

/// <summary>
/// Response containing password reset information
/// </summary>
public sealed record ForgotPasswordResponse(
    string Email,
    string ResetToken,
    DateTime ExpiresAt);
