using FluentValidation;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Commands.CreatePost;

/// <summary>
/// Validator for CreatePostCommand
/// </summary>
public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters")
            .MaximumLength(50000).WithMessage("Content must not exceed 50000 characters");

        RuleFor(x => x.Type)
            .Must(type => Enum.IsDefined(typeof(PostType), type))
            .WithMessage("Invalid post type");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count() <= 10)
            .WithMessage("A post can have at most 10 tags");
    }
}
