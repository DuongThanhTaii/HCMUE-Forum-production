using FluentValidation;
using UniHub.Forum.Domain.Comments.ValueObjects;

namespace UniHub.Forum.Application.Commands.AddComment;

/// <summary>
/// Validator for AddCommentCommand
/// </summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage("Post ID is required");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MinimumLength(CommentContent.MinLength)
            .WithMessage($"Comment content must be at least {CommentContent.MinLength} character")
            .MaximumLength(CommentContent.MaxLength)
            .WithMessage($"Comment content cannot exceed {CommentContent.MaxLength} characters");
    }
}
