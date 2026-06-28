using FluentValidation;

namespace UniHub.Forum.Application.Commands.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
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
