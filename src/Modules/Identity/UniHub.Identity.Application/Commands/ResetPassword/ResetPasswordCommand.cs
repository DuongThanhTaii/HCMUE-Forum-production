using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.ResetPassword;

/// <summary>
/// Command to reset user password with a valid token
/// </summary>
public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword) : ICommand;
