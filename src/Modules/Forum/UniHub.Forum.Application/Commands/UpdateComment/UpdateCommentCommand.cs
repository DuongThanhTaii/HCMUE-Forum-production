using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.UpdateComment;

/// <summary>
/// Command to update an existing comment
/// </summary>
public sealed record UpdateCommentCommand(
    Guid CommentId,
    string Content,
    Guid RequestingUserId) : ICommand;
