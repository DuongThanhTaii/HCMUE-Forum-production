using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.UpdateComment;

/// <summary>
/// Handler for updating an existing comment
/// </summary>
public sealed class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand>
{
    private readonly ICommentRepository _commentRepository;

    public UpdateCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        // Get comment
        var commentId = new CommentId(request.CommentId);
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.CommentNotFound);
        }

        // Check authorization - only author can update
        if (comment.AuthorId != request.RequestingUserId)
        {
            return Result.Failure(CommentErrors.UnauthorizedAccess);
        }

        // Create new content value object
        var contentResult = CommentContent.Create(request.Content);
        if (contentResult.IsFailure)
        {
            return Result.Failure(contentResult.Error);
        }

        // Update comment
        var updateResult = comment.Update(contentResult.Value);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        // Update repository
        await _commentRepository.UpdateAsync(comment, cancellationToken);

        return Result.Success();
    }
}
