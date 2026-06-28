using FluentValidation;

namespace UniHub.Forum.Application.Commands.PinPost;

/// <summary>
/// Validator for PinPostCommand
/// </summary>
public sealed class PinPostCommandValidator : AbstractValidator<PinPostCommand>
{
    public PinPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required");
    }
}
