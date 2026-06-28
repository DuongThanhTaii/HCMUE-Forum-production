using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.CreatePost;

/// <summary>
/// Handler for creating a new post
/// </summary>
public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IThreadChannelRepository _threadChannelRepository;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        IThreadChannelRepository threadChannelRepository)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _threadChannelRepository = threadChannelRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists if provided
        if (request.CategoryId.HasValue)
        {
            var categoryId = new CategoryId(request.CategoryId.Value);
            var categoryExists = await _categoryRepository.ExistsAsync(categoryId, cancellationToken);
            if (!categoryExists)
            {
                return Result.Failure<Guid>(PostErrors.CategoryNotFound);
            }
        }

        if (request.ThreadChannelId.HasValue)
        {
            var threadChannel = await _threadChannelRepository.GetByIdAsync(request.ThreadChannelId.Value, cancellationToken);
            if (threadChannel is null || !threadChannel.IsActive)
            {
                return Result.Failure<Guid>(PostErrors.ThreadChannelNotFound);
            }
        }

        // Create title value object
        var titleResult = PostTitle.Create(request.Title);
        if (titleResult.IsFailure)
        {
            return Result.Failure<Guid>(titleResult.Error);
        }

        // Create content value object
        var contentResult = PostContent.Create(request.Content);
        if (contentResult.IsFailure)
        {
            return Result.Failure<Guid>(contentResult.Error);
        }

        // Validate post type


        var postType = (PostType)request.Type;

        // Create post
        var postResult = Post.Create(
            titleResult.Value,
            contentResult.Value,
            postType,
            request.AuthorId,
            request.CategoryId,
            request.Tags,
            request.ThreadChannelId);

        if (postResult.IsFailure)
        {
            return Result.Failure<Guid>(postResult.Error);
        }

        // Save post
        await _postRepository.AddAsync(postResult.Value, cancellationToken);

        return Result.Success(postResult.Value.Id.Value);
    }
}
