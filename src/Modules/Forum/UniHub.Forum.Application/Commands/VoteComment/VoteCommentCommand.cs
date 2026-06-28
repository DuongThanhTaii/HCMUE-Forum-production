using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.VoteComment;

/// <summary>
/// Command to vote on a comment (upvote or downvote)
/// If user votes the same type again, the vote is removed
/// If user votes different type, the vote is changed
/// </summary>
public sealed record VoteCommentCommand(
    Guid CommentId,
    Guid UserId,
    VoteType VoteType) : ICommand;
