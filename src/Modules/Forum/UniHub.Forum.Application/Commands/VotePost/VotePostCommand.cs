using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Commands.VotePost;

/// <summary>
/// Command to vote on a post (upvote or downvote)
/// If user votes the same type again, the vote is removed
/// If user votes different type, the vote is changed
/// </summary>
public sealed record VotePostCommand(
    Guid PostId,
    Guid UserId,
    VoteType VoteType) : ICommand;
