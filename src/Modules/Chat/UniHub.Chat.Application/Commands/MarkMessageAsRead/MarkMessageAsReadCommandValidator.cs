using FluentValidation;

namespace UniHub.Chat.Application.Commands.MarkMessageAsRead;

/// <summary>
/// Validator for MarkMessageAsReadCommand
/// </summary>
public sealed class MarkMessageAsReadCommandValidator : AbstractValidator<MarkMessageAsReadCommand>
{
    public MarkMessageAsReadCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
