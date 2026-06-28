namespace UniHub.Forum.Application.Queries.GetComments;

/// <summary>
/// Represents a comment in the list
/// </summary>
public sealed record CommentItem
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
    public int VoteScore { get; init; }
    public int? CurrentUserVote { get; init; }
    public bool IsAcceptedAnswer { get; init; }
    public bool IsPinned { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
