using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.AddParticipant;

/// <summary>
/// Handler for adding a participant to a group conversation
/// </summary>
public sealed class AddParticipantCommandHandler : ICommandHandler<AddParticipantCommand>
{
    private readonly IConversationRepository _conversationRepository;

    public AddParticipantCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result> Handle(
        AddParticipantCommand request,
        CancellationToken cancellationToken)
    {
        // Get conversation
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        // Add participant using domain method
        var result = conversation.AddParticipant(request.ParticipantId, request.AddedBy);

        if (result.IsFailure)
        {
            return result;
        }

        // Update conversation
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return Result.Success();
    }
}
