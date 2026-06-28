namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Request for user registration
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string? Bio = null);
