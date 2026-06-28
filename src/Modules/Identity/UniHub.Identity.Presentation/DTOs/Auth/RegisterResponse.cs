namespace UniHub.Identity.Presentation.DTOs.Auth;

/// <summary>
/// Response after successful registration
/// </summary>
public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string FullName);
