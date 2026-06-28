using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.RemoveBadge;

/// <summary>
/// Command to remove an official badge from a user
/// </summary>
public sealed record RemoveBadgeCommand(Guid UserId) : ICommand;
