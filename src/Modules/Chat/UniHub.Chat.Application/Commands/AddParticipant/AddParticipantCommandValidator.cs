using FluentValidation;

namespace UniHub.Chat.Application.Commands.AddParticipant;

/// <summary>
/// Validator for AddParticipantCommand
/// </summary>
public sealed class AddParticipantCommandValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required");

        RuleFor(x => x.ParticipantId)
            .NotEmpty()
            .WithMessage("ParticipantId is required");

        RuleFor(x => x.AddedBy)
            .NotEmpty()
            .WithMessage("AddedBy is required");
    }
}
