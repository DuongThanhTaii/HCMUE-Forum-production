using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.AcceptAnswer;

/// <summary>
/// Command to accept a comment as the answer to a question post
/// </summary>
public sealed record AcceptAnswerCommand(
    Guid CommentId,
    Guid PostId,
    Guid RequestingUserId) : ICommand;
