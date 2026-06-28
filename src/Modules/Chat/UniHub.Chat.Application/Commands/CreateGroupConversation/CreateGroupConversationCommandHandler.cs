using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.CreateGroupConversation;

/// <summary>
/// Handler for creating a group conversation
/// </summary>
public sealed class CreateGroupConversationCommandHandler : ICommandHandler<CreateGroupConversationCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;

    public CreateGroupConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<Guid>> Handle(
        CreateGroupConversationCommand request,
        CancellationToken cancellationToken)
    {
        // Create group conversation using domain factory
        var conversationResult = Conversation.CreateGroup(
            request.Title,
            request.ParticipantIds,
            request.CreatorId);

        if (conversationResult.IsFailure)
        {
            return Result.Failure<Guid>(conversationResult.Error);
        }

        var conversation = conversationResult.Value;

        // Persist conversation
        await _conversationRepository.AddAsync(conversation, cancellationToken);

        return Result.Success(conversation.Id.Value);
    }
}
