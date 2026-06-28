using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.PinComment;

public sealed record PinCommentCommand(
    Guid CommentId,
    Guid PostId,
    Guid RequestingUserId,
    bool HasModerationPrivilege) : ICommand;
