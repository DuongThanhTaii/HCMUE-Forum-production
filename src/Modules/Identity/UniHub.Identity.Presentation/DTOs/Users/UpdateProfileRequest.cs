namespace UniHub.Identity.Presentation.DTOs.Users;

/// <summary>
/// Request to update user profile
/// </summary>
public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Bio);
