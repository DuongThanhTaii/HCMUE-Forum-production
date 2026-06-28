using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.AddComment;

/// <summary>
/// Command to add a new comment to a post
/// </summary>
public sealed record AddCommentCommand(
    Guid PostId,
    Guid AuthorId,
    string Content,
    Guid? ParentCommentId = null) : ICommand<Guid>;
