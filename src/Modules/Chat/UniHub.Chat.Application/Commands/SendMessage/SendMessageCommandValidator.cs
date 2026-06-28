using FluentValidation;

namespace UniHub.Chat.Application.Commands.SendMessage;

/// <summary>
/// Validator for SendMessageCommand
/// </summary>
public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required");

        RuleFor(x => x.SenderId)
            .NotEmpty()
            .WithMessage("SenderId is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters");
    }
}
