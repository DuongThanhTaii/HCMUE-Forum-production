using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.VotePost;

/// <summary>
/// Handler for voting on a post
/// Implements the logic: vote same type = remove, vote different type = change
/// </summary>
public sealed class VotePostCommandHandler : ICommandHandler<VotePostCommand>
{
    private readonly IPostRepository _postRepository;

    public VotePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(VotePostCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(PostErrors.PostNotFound);
        }

        // Check if user already voted
        var existingVote = post.Votes.FirstOrDefault(v => v.UserId == request.UserId);

        if (existingVote is null)
        {
            // No existing vote, add new vote
            var addResult = post.AddVote(request.UserId, request.VoteType);
            if (addResult.IsFailure)
            {
                return Result.Failure(addResult.Error);
            }
        }
        else if (existingVote.Type == request.VoteType)
        {
            // Same vote type, remove the vote
            var removeResult = post.RemoveVote(request.UserId);
            if (removeResult.IsFailure)
            {
                return Result.Failure(removeResult.Error);
            }
        }
        else
        {
            // Different vote type, change the vote
            var changeResult = post.ChangeVote(request.UserId, request.VoteType);
            if (changeResult.IsFailure)
            {
                return Result.Failure(changeResult.Error);
            }
        }

        // Update repository
        await _postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }
}
