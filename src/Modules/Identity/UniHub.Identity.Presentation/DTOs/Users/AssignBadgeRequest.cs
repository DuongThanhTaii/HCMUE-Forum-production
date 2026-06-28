namespace UniHub.Identity.Presentation.DTOs.Users;

/// <summary>
/// Request to assign official badge to user
/// </summary>
public sealed record AssignBadgeRequest(
    string BadgeType,
    string BadgeName,
    string? Description);
