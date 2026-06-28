using FluentValidation;

namespace UniHub.Chat.Application.Commands.JoinChannel;

/// <summary>
/// Validator for JoinChannelCommand
/// </summary>
public sealed class JoinChannelCommandValidator : AbstractValidator<JoinChannelCommand>
{
    public JoinChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .WithMessage("ChannelId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
