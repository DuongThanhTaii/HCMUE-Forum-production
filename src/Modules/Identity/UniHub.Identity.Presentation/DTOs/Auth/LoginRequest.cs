namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Request for user login
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);
