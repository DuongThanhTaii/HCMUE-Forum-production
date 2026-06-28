using FluentValidation;
using UniHub.Forum.Domain.Comments.ValueObjects;

namespace UniHub.Forum.Application.Commands.UpdateComment;

/// <summary>
/// Validator for UpdateCommentCommand
/// </summary>
public sealed class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage("Comment ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MinimumLength(CommentContent.MinLength)
            .WithMessage($"Comment content must be at least {CommentContent.MinLength} character")
            .MaximumLength(CommentContent.MaxLength)
            .WithMessage($"Comment content cannot exceed {CommentContent.MaxLength} characters");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty()
            .WithMessage("Requesting user ID is required");
    }
}
