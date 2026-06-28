using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.DeletePost;

/// <summary>
/// Handler for deleting a post
/// </summary>
public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(PostErrors.PostNotFound);
        }

        // Check authorization (only author can delete)
        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(PostErrors.UnauthorizedAccess);
        }

        // Delete post (soft delete via domain method)
        var deleteResult = post.Delete();
        if (deleteResult.IsFailure)
        {
            return Result.Failure(deleteResult.Error);
        }

        // Save changes
        await _postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }
}
