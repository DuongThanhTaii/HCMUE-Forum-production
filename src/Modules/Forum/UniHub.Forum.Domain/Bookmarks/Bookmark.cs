using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Domain.Bookmarks;

/// <summary>
/// Represents a user's bookmark of a post
/// </summary>
public sealed class Bookmark
{
    public PostId PostId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Bookmark()
    {
        PostId = null!;
    }

    private Bookmark(PostId postId, Guid userId)
    {
        PostId = postId;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Bookmark Create(PostId postId, Guid userId)
    {
        return new Bookmark(postId, userId);
    }
}
