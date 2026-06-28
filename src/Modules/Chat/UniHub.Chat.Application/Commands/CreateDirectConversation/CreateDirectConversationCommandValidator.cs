using FluentValidation;

namespace UniHub.Chat.Application.Commands.CreateDirectConversation;

/// <summary>
/// Validator for CreateDirectConversationCommand
/// </summary>
public sealed class CreateDirectConversationCommandValidator 
    : AbstractValidator<CreateDirectConversationCommand>
{
    public CreateDirectConversationCommandValidator()
    {
        RuleFor(x => x.User1Id)
            .NotEmpty()
            .WithMessage("User1Id is required");

        RuleFor(x => x.User2Id)
            .NotEmpty()
            .WithMessage("User2Id is required");

        RuleFor(x => x.CreatorId)
            .NotEmpty()
            .WithMessage("CreatorId is required");

        RuleFor(x => x)
            .Must(x => x.User1Id != x.User2Id)
            .WithMessage("Cannot create conversation with same user");

        RuleFor(x => x)
            .Must(x => x.CreatorId == x.User1Id || x.CreatorId == x.User2Id)
            .WithMessage("Creator must be one of the participants");
    }
}
