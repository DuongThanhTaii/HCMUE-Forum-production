namespace UniHub.Forum.Presentation.DTOs.Comments;

/// <summary>
/// Request to update a comment
/// </summary>
public sealed record UpdateCommentRequest
{
    /// <summary>
    /// New comment content
    /// </summary>
    public string Content { get; init; } = string.Empty;
}
