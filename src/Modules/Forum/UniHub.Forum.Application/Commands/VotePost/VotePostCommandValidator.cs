using FluentValidation;

namespace UniHub.Forum.Application.Commands.VotePost;

/// <summary>
/// Validator for VotePostCommand
/// </summary>
public sealed class VotePostCommandValidator : AbstractValidator<VotePostCommand>
{
    public VotePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.VoteType)
            .IsInEnum()
            .WithMessage("Vote type must be Upvote (1) or Downvote (-1)");
    }
}
