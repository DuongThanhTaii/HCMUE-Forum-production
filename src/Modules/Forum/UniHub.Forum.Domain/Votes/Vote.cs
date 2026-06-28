using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Votes;

/// <summary>
/// Represents a vote on a post or comment.
/// Votes can be either upvote or downvote.
/// </summary>
public sealed class Vote : ValueObject
{
    public Guid UserId { get; private set; }
    public VoteType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private Vote()
    {
    }

    private Vote(Guid userId, VoteType type)
    {
        UserId = userId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    /// <summary>
    /// Creates a new vote.
    /// </summary>
    public static Result<Vote> Create(Guid userId, VoteType voteType)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Vote>(new Error("Vote.InvalidUserId", "User ID cannot be empty"));
        }

        if (!Enum.IsDefined(typeof(VoteType), voteType))
        {
            return Result.Failure<Vote>(new Error("Vote.InvalidVoteType", "Invalid vote type"));
        }

        return Result.Success(new Vote(userId, voteType));
    }

    /// <summary>
    /// Changes the vote type (e.g., from upvote to downvote or vice versa).
    /// </summary>
    public Result<Vote> Change(VoteType newVoteType)
    {
        if (!Enum.IsDefined(typeof(VoteType), newVoteType))
        {
            return Result.Failure<Vote>(new Error("Vote.InvalidVoteType", "Invalid vote type"));
        }

        if (Type == newVoteType)
        {
            return Result.Failure<Vote>(new Error("Vote.SameVoteType", "Vote type is already set to this value"));
        }

        var changedVote = new Vote(UserId, newVoteType)
        {
            CreatedAt = CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        return Result.Success(changedVote);
    }

    /// <summary>
    /// Gets the score value for this vote (1 for upvote, -1 for downvote).
    /// </summary>
    public int GetScoreValue() => (int)Type;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return Type;
    }
}
