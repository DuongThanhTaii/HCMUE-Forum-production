using FluentValidation;

namespace UniHub.Chat.Application.Commands.EditMessage;

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(5000);
    }
}
