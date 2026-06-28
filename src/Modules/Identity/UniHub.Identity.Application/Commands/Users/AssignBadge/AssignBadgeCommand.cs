using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.AssignBadge;

/// <summary>
/// Command to assign an official badge to a user
/// </summary>
public sealed record AssignBadgeCommand(
    Guid UserId,
    BadgeType BadgeType,
    string BadgeName,
    string VerifiedBy,
    string? Description = null) : ICommand;
