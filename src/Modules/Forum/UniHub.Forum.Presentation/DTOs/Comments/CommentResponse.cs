namespace UniHub.Forum.Presentation.DTOs.Comments;

/// <summary>
/// Response containing comment details
/// </summary>
public sealed record CommentResponse
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public string? AuthorName { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid? ParentCommentId { get; init; }
    public int VoteScore { get; init; }
    public int? CurrentUserVote { get; init; }
    public bool IsAcceptedAnswer { get; init; }
    public bool IsPinned { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
