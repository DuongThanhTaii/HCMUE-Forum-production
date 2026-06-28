using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.UnblockUser;

public sealed record UnblockUserCommand(Guid BlockerUserId, Guid BlockedUserId) : ICommand;
