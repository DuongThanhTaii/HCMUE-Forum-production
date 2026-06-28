using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.PinPost;

/// <summary>
/// Handler for pinning a post
/// Note: Pinning is typically a moderator action, but for simplicity we check author permission here.
/// In a real application, you would check for moderator role or permissions.
/// </summary>
public sealed class PinPostCommandHandler : ICommandHandler<PinPostCommand>
{
    private readonly IPostRepository _postRepository;

    public PinPostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(PinPostCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(PostErrors.PostNotFound);
        }

        // Note: In a real application, you would check if the requesting user has moderator permissions
        // For now, we allow only the author to pin their own post
        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(PostErrors.UnauthorizedAccess);
        }

        // Pin post
        var pinResult = post.Pin();
        if (pinResult.IsFailure)
        {
            return Result.Failure(pinResult.Error);
        }

        // Save changes
        await _postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }
}
