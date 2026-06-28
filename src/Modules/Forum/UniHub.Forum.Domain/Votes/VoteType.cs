namespace UniHub.Forum.Domain.Votes;

/// <summary>
/// Represents the type of vote.
/// Values are intentionally set to 1 and -1 for direct score calculation.
/// </summary>
public enum VoteType
{
    /// <summary>
    /// Downvote decreases the score by 1
    /// </summary>
    Downvote = -1,

    /// <summary>
    /// Upvote increases the score by 1
    /// </summary>
    Upvote = 1
}
