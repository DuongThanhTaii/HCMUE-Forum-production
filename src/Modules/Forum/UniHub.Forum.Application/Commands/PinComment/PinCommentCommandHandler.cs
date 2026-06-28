using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.PinComment;

public sealed class PinCommentCommandHandler : ICommandHandler<PinCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IThreadChannelRepository _threadChannelRepository;

    public PinCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IThreadChannelRepository threadChannelRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _threadChannelRepository = threadChannelRepository;
    }

    public async Task<Result> Handle(PinCommentCommand request, CancellationToken cancellationToken)
    {
        var commentId = new CommentId(request.CommentId);
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.CommentNotFound);
        }

        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure(CommentErrors.PostNotFound);
        }

        if (comment.PostId != postId)
        {
            return Result.Failure(new Error("Comment.WrongPost", "This comment does not belong to the specified post"));
        }

        var isPostOwner = post.AuthorId == request.RequestingUserId;
        if (!isPostOwner && !request.HasModerationPrivilege)
        {
            return Result.Failure(CommentErrors.UnauthorizedAccess);
        }

        if (post.ThreadChannelId.HasValue)
        {
            var threadChannel = await _threadChannelRepository.GetByIdAsync(post.ThreadChannelId.Value, cancellationToken);
            if (threadChannel is not null && !threadChannel.AllowPinnedComments)
            {
                return Result.Failure(new Error(
                    "ThreadChannel.PinCommentDisabled",
                    "Pin comment is disabled by this thread channel policy."));
            }
        }

        var pinResult = comment.IsPinned
            ? comment.Unpin()
            : comment.Pin();
        if (pinResult.IsFailure)
        {
            return Result.Failure(pinResult.Error);
        }

        await _commentRepository.UpdateAsync(comment, cancellationToken);
        return Result.Success();
    }
}
