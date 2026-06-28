using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Posts;
using UniHub.Notification.Application.Abstractions;

namespace UniHub.API.Services;

/// <summary>
/// Adapter: resolves post author for Notification module without cross-layer coupling.
/// </summary>
public sealed class PostAuthorLookup : IPostAuthorLookup
{
    private readonly IPostRepository _postRepository;

    public PostAuthorLookup(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<(Guid AuthorId, string Title)?> GetAuthorAsync(PostId postId, CancellationToken ct = default)
    {
        var post = await _postRepository.GetByIdAsync(postId, ct);
        if (post is null) return null;
        return (post.AuthorId, post.Title.Value);
    }
}
