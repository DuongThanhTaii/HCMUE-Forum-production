using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.DeleteComment;

/// <summary>
/// Handler for deleting a comment (soft delete)
/// </summary>
public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        // Get comment
        var commentId = new CommentId(request.CommentId);
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.CommentNotFound);
        }

        // Check authorization - only author can delete
        if (comment.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(CommentErrors.UnauthorizedAccess);
        }

        // Delete comment (soft delete via domain logic)
        var deleteResult = comment.Delete();
        if (deleteResult.IsFailure)
        {
            return Result.Failure(deleteResult.Error);
        }

        // Update repository
        await _commentRepository.UpdateAsync(comment, cancellationToken);

        return Result.Success();
    }
}
