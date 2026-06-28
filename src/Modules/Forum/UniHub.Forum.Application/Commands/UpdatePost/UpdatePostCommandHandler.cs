using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.UpdatePost;

/// <summary>
/// Handler for updating an existing post
/// </summary>
public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IThreadChannelRepository _threadChannelRepository;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        IThreadChannelRepository threadChannelRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _threadChannelRepository = threadChannelRepository;
    }

    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(PostErrors.PostNotFound);
        }

        // Check authorization (only author can update)
        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(PostErrors.UnauthorizedAccess);
        }

        // Validate category exists if provided
        if (request.CategoryId.HasValue)
        {
            var categoryId = new CategoryId(request.CategoryId.Value);
            var categoryExists = await _categoryRepository.ExistsAsync(categoryId, cancellationToken);
            if (!categoryExists)
            {
                return Result.Failure(PostErrors.CategoryNotFound);
            }
        }

        if (request.ThreadChannelId.HasValue)
        {
            var threadChannel = await _threadChannelRepository.GetByIdAsync(request.ThreadChannelId.Value, cancellationToken);
            if (threadChannel is null || !threadChannel.IsActive)
            {
                return Result.Failure(PostErrors.ThreadChannelNotFound);
            }
        }

        // Create title value object
        var titleResult = PostTitle.Create(request.Title);
        if (titleResult.IsFailure)
        {
            return Result.Failure(titleResult.Error);
        }

        // Create content value object
        var contentResult = PostContent.Create(request.Content);
        if (contentResult.IsFailure)
        {
            return Result.Failure(contentResult.Error);
        }

        // Update post
        var updateResult = post.Update(
            titleResult.Value,
            contentResult.Value,
            request.CategoryId,
            request.ThreadChannelId,
            request.Tags);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        // Save changes
        await _postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }
}
