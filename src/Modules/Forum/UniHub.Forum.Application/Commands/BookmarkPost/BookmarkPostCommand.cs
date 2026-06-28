using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.BookmarkPost;

public sealed record BookmarkPostCommand(
    Guid PostId,
    Guid UserId) : ICommand;
