using FluentValidation;

namespace UniHub.Forum.Application.Commands.AcceptAnswer;

/// <summary>
/// Validator for AcceptAnswerCommand
/// </summary>
public sealed class AcceptAnswerCommandValidator : AbstractValidator<AcceptAnswerCommand>
{
    public AcceptAnswerCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");

        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required");
    }
}
