using FluentValidation;

namespace UniHub.Forum.Application.Commands.UpdateTag;

public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.TagId)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Tag name can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
