using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.DeleteComment;

/// <summary>
/// Command to delete a comment (soft delete)
/// </summary>
public sealed record DeleteCommentCommand(
    Guid CommentId,
    Guid RequestingUserId) : ICommand;
