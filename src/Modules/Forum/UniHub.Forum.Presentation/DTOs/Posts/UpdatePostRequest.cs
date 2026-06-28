namespace UniHub.Forum.Presentation.DTOs.Posts;

/// <summary>
/// Request to update an existing post
/// </summary>
public sealed record UpdatePostRequest
{
    /// <summary>
    /// New post title
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// New post content
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// New category ID
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// New tags
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }

    /// <summary>
    /// New thread channel.
    /// </summary>
    public Guid? ThreadChannelId { get; init; }
}
