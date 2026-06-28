using FluentValidation;

namespace UniHub.Forum.Application.Commands.DeletePost;

/// <summary>
/// Validator for DeletePostCommand
/// </summary>
public sealed class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");
    }
}
