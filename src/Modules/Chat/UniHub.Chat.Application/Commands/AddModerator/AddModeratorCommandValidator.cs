using FluentValidation;

namespace UniHub.Chat.Application.Commands.AddModerator;

/// <summary>
/// Validator for AddModeratorCommand
/// </summary>
public sealed class AddModeratorCommandValidator : AbstractValidator<AddModeratorCommand>
{
    public AddModeratorCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .WithMessage("ChannelId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.AddedBy)
            .NotEmpty()
            .WithMessage("AddedBy is required");
    }
}
