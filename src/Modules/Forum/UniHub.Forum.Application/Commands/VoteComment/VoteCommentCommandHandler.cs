using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Commands.VoteComment;

/// <summary>
/// Handler for voting on a comment
/// Implements the logic: vote same type = remove, vote different type = change
/// </summary>
public sealed class VoteCommentCommandHandler : ICommandHandler<VoteCommentCommand>
{
    private readonly ICommentRepository _commentRepository;

    public VoteCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result> Handle(VoteCommentCommand request, CancellationToken cancellationToken)
    {
        // Get comment
        var commentId = new CommentId(request.CommentId);
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.CommentNotFound);
        }

        // Check if user already voted
        var existingVote = comment.Votes.FirstOrDefault(v => v.UserId == request.UserId);

        if (existingVote is null)
        {
            // No existing vote, add new vote
            var addResult = comment.AddVote(request.UserId, request.VoteType);
            if (addResult.IsFailure)
            {
                return Result.Failure(addResult.Error);
            }
        }
        else if (existingVote.Type == request.VoteType)
        {
            // Same vote type, remove the vote
            var removeResult = comment.RemoveVote(request.UserId);
            if (removeResult.IsFailure)
            {
                return Result.Failure(removeResult.Error);
            }
        }
        else
        {
            // Different vote type, change the vote
            var changeResult = comment.ChangeVote(request.UserId, request.VoteType);
            if (changeResult.IsFailure)
            {
                return Result.Failure(changeResult.Error);
            }
        }

        // Update repository
        await _commentRepository.UpdateAsync(comment, cancellationToken);

        return Result.Success();
    }
}
