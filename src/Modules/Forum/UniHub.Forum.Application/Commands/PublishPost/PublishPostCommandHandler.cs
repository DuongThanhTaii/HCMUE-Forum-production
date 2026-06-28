using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.PublishPost;

/// <summary>
/// Handler for publishing a post
/// </summary>
public sealed class PublishPostCommandHandler : ICommandHandler<PublishPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IThreadChannelRepository _threadChannelRepository;

    public PublishPostCommandHandler(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        IThreadChannelRepository threadChannelRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _threadChannelRepository = threadChannelRepository;
    }

    public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(PostErrors.PostNotFound);
        }

        if (!await CanPublishAsync(post, request, cancellationToken))
        {
            return Result.Failure(PostErrors.UnauthorizedAccess);
        }

        // Publish post
        var publishResult = post.Publish();
        if (publishResult.IsFailure)
        {
            return Result.Failure(publishResult.Error);
        }

        // Save changes
        await _postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }

    private async Task<bool> CanPublishAsync(
        Post post,
        PublishPostCommand request,
        CancellationToken cancellationToken)
    {
        switch (request.Actor)
        {
            case PostPublishActor.Author:
                return post.AuthorId == request.RequestingUserId;
            case PostPublishActor.Admin:
                return true;
            case PostPublishActor.Moderator:
                if (post.ThreadChannelId.HasValue)
                {
                    var threadChannel = await _threadChannelRepository.GetByIdAsync(post.ThreadChannelId.Value, cancellationToken);
                    if (threadChannel is not null && !threadChannel.AllowModeratorActions)
                    {
                        return false;
                    }
                }

                if (post.CategoryId is null)
                {
                    return false;
                }

                // Keep behavior consistent with moderation inbox fallback:
                // if moderator has no explicit category assignment yet, allow global moderation.
                var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
                var hasAnyAssignedScope = allCategories.Any(c => c.ModeratorIds.Contains(request.RequestingUserId));
                if (!hasAnyAssignedScope)
                {
                    return true;
                }

                var category = await _categoryRepository.GetByIdAsync(
                    new CategoryId(post.CategoryId.Value),
                    cancellationToken);
                return category is not null && category.ModeratorIds.Contains(request.RequestingUserId);
            default:
                return false;
        }
    }
}
