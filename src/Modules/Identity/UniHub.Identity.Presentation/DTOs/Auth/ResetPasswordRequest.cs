namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Request to reset password
/// </summary>
public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);
