using FluentValidation;

namespace UniHub.Forum.Application.Commands.UpdatePost;

/// <summary>
/// Validator for UpdatePostCommand
/// </summary>
public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .MaximumLength(50000).WithMessage("Content must not exceed 50000 characters");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count() <= 10)
            .WithMessage("A post can have at most 10 tags");
    }
}
