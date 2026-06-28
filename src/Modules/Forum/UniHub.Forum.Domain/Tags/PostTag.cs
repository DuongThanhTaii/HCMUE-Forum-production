using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Domain.Tags;

/// <summary>
/// Join entity for many-to-many relationship between Posts and Tags
/// </summary>
public sealed class PostTag
{
    public PostId PostId { get; private set; }
    public TagId TagId { get; private set; }
    public DateTime AddedAt { get; private set; }

    // EF Core constructor
    private PostTag()
    {
        PostId = null!;
        TagId = null!;
    }

    private PostTag(PostId postId, TagId tagId)
    {
        PostId = postId;
        TagId = tagId;
        AddedAt = DateTime.UtcNow;
    }

    public static PostTag Create(PostId postId, TagId tagId)
    {
        return new PostTag(postId, tagId);
    }
}
