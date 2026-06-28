namespace UniHub.Forum.Presentation.DTOs.Comments;

/// <summary>
/// Request to add a comment to a post
/// </summary>
public sealed record AddCommentRequest
{
    /// <summary>
    /// Comment content
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Optional parent comment ID for nested replies
    /// </summary>
    public Guid? ParentCommentId { get; init; }
}
