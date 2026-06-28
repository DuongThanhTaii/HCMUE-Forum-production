using FluentValidation;

namespace UniHub.Forum.Application.Commands.DeleteComment;

/// <summary>
/// Validator for DeleteCommentCommand
/// </summary>
public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required");
    }
}
