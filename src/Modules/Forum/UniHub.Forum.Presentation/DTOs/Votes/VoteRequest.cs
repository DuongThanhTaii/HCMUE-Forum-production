namespace UniHub.Forum.Presentation.DTOs.Votes;

/// <summary>
/// Request to vote on a post or comment
/// </summary>
public sealed record VoteRequest
{
    /// <summary>
    /// Vote type (1=Upvote, 2=Downvote)
    /// </summary>
    public int VoteType { get; init; }
}
