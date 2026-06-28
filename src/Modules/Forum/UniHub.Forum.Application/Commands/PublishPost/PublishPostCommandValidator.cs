using FluentValidation;

namespace UniHub.Forum.Application.Commands.PublishPost;

/// <summary>
/// Validator for PublishPostCommand
/// </summary>
public sealed class PublishPostCommandValidator : AbstractValidator<PublishPostCommand>
{
    public PublishPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");
    }
}
