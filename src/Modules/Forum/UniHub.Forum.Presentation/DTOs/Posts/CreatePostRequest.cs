namespace UniHub.Forum.Presentation.DTOs.Posts;

/// <summary>
/// Request to create a new post
/// </summary>
public sealed record CreatePostRequest
{
    /// <summary>
    /// Post title
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Post content
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Post type (1=Discussion, 2=Question)
    /// </summary>
    public int Type { get; init; }

    /// <summary>
    /// Optional category ID  
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Optional tags
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }

    /// <summary>
    /// Optional thread channel ID for VOZ-style thread routing.
    /// </summary>
    public Guid? ThreadChannelId { get; init; }
}
