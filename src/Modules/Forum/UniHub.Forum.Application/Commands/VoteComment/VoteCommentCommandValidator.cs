using FluentValidation;

namespace UniHub.Forum.Application.Commands.VoteComment;

/// <summary>
/// Validator for VoteCommentCommand
/// </summary>
public sealed class VoteCommentCommandValidator : AbstractValidator<VoteCommentCommand>
{
    public VoteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.VoteType)
            .IsInEnum()
            .WithMessage("Vote type must be Upvote (1) or Downvote (-1)");
    }
}
