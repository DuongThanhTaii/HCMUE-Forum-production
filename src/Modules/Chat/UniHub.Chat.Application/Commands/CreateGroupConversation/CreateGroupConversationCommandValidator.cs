using FluentValidation;

namespace UniHub.Chat.Application.Commands.CreateGroupConversation;

/// <summary>
/// Validator for CreateGroupConversationCommand
/// </summary>
public sealed class CreateGroupConversationCommandValidator : AbstractValidator<CreateGroupConversationCommand>
{
    public CreateGroupConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MinimumLength(3)
            .WithMessage("Title must be at least 3 characters")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.ParticipantIds)
            .NotEmpty()
            .WithMessage("Participant list is required")
            .Must(p => p != null && p.Count >= 2)
            .WithMessage("Group conversation must have at least 2 participants");

        RuleFor(x => x.CreatorId)
            .NotEmpty()
            .WithMessage("CreatorId is required");

        RuleFor(x => x)
            .Must(x => x.ParticipantIds != null && x.ParticipantIds.Contains(x.CreatorId))
            .WithMessage("Creator must be one of the participants");

        RuleFor(x => x.ParticipantIds)
            .Must(p => p == null || p.Distinct().Count() == p.Count)
            .WithMessage("Participant list cannot contain duplicates");
    }
}
