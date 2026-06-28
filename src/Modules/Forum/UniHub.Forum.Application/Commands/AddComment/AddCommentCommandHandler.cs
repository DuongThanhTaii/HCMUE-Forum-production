using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.AddComment;

/// <summary>
/// Handler for adding a new comment to a post
/// </summary>
public sealed class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, Guid>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public AddCommentCommandHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        // Verify post exists
        var postId = new PostId(request.PostId);
        var post = await _postRepository.GetByIdAsync(postId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<Guid>(CommentErrors.PostNotFound);
        }

        // If parent comment is specified, verify it exists
        CommentId? parentCommentId = null;
        if (request.ParentCommentId.HasValue)
        {
            parentCommentId = new CommentId(request.ParentCommentId.Value);
            var parentComment = await _commentRepository.GetByIdAsync(parentCommentId, cancellationToken);
            if (parentComment is null)
            {
                return Result.Failure<Guid>(CommentErrors.ParentCommentNotFound);
            }
        }

        // Create content value object
        var contentResult = CommentContent.Create(request.Content);
        if (contentResult.IsFailure)
        {
            return Result.Failure<Guid>(contentResult.Error);
        }

        // Create comment aggregate
        var commentResult = Comment.Create(
            postId,
            request.AuthorId,
            contentResult.Value,
            parentCommentId);

        if (commentResult.IsFailure)
        {
            return Result.Failure<Guid>(commentResult.Error);
        }

        // Add to repository
        await _commentRepository.AddAsync(commentResult.Value, cancellationToken);

        return Result.Success(commentResult.Value.Id.Value);
    }
}
