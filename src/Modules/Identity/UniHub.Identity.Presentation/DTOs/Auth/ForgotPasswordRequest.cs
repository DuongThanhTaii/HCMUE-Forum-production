namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Request for forgot password
/// </summary>
public sealed record ForgotPasswordRequest(
    string Email);
