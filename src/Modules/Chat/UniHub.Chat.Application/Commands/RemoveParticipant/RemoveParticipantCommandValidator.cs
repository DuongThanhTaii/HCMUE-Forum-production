using FluentValidation;

namespace UniHub.Chat.Application.Commands.RemoveParticipant;

/// <summary>
/// Validator for RemoveParticipantCommand
/// </summary>
public sealed class RemoveParticipantCommandValidator : AbstractValidator<RemoveParticipantCommand>
{
    public RemoveParticipantCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("ConversationId is required");

        RuleFor(x => x.ParticipantId)
            .NotEmpty()
            .WithMessage("ParticipantId is required");

        RuleFor(x => x.RemovedBy)
            .NotEmpty()
            .WithMessage("RemovedBy is required");
    }
}
