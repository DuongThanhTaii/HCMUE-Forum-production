using FluentValidation;

namespace UniHub.Chat.Application.Commands.LeaveChannel;

/// <summary>
/// Validator for LeaveChannelCommand
/// </summary>
public sealed class LeaveChannelCommandValidator : AbstractValidator<LeaveChannelCommand>
{
    public LeaveChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .WithMessage("ChannelId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
