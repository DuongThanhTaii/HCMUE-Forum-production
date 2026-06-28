using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.UnbookmarkPost;

public sealed record UnbookmarkPostCommand(
    Guid PostId,
    Guid UserId) : ICommand;
