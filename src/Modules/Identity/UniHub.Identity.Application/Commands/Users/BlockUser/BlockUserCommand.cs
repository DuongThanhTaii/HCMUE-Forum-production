using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.BlockUser;

public sealed record BlockUserCommand(Guid BlockerUserId, Guid BlockedUserId) : ICommand;
