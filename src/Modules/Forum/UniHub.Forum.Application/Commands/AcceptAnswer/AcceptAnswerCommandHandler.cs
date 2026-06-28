using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.AcceptAnswer;

/// <summary>
/// Handler for accepting a comment as the answer to a question post
/// </summary>
public sealed class AcceptAnswerCommandHandler : ICommandHandler<AcceptAnswerCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IThreadChannelRepository _threadChannelRepository;

    public AcceptAnswerCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IThreadChannelRepository threadChannelRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _threadChannelRepository = threadChannelRepository;
    }

    public async Task<Result> Handle(AcceptAnswerCommand request, CancellationToken cancellationToken)
    {
        // Get post
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(CommentErrors.PostNotFound);
        }

        // Check authorization - only post author can accept answers
        if (post.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(CommentErrors.UnauthorizedAccess);
        }

        if (post.ThreadChannelId.HasValue)
        {
            var threadChannel = await _threadChannelRepository.GetByIdAsync(post.ThreadChannelId.Value, cancellationToken);
            if (threadChannel is not null && !threadChannel.AllowAcceptedAnswers)
            {
                return Result.Failure(new Error(
                    "ThreadChannel.AcceptedAnswerDisabled",
                    "Accepted answer is disabled by this thread channel policy."));
            }
        }

        // Get comment
        var commentId = new CommentId(request.CommentId);
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.CommentNotFound);
        }

        // Verify comment belongs to the post
        if (comment.PostId != postId)
        {
            return Result.Failure(new Error(
                "Comment.WrongPost",
                "This comment does not belong to the specified post"));
        }

        // Accept as answer
        var acceptResult = comment.AcceptAsAnswer();
        if (acceptResult.IsFailure)
        {
            return Result.Failure(acceptResult.Error);
        }

        // Update repository
        await _commentRepository.UpdateAsync(comment, cancellationToken);

        return Result.Success();
    }
}
