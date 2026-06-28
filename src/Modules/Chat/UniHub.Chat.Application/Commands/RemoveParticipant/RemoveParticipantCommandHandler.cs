using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.RemoveParticipant;

/// <summary>
/// Handler for removing a participant from a group conversation
/// </summary>
public sealed class RemoveParticipantCommandHandler : ICommandHandler<RemoveParticipantCommand>
{
    private readonly IConversationRepository _conversationRepository;

    public RemoveParticipantCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result> Handle(
        RemoveParticipantCommand request,
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

        // Remove participant using domain method
        var result = conversation.RemoveParticipant(request.ParticipantId, request.RemovedBy);

        if (result.IsFailure)
        {
            return result;
        }

        // Update conversation
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return Result.Success();
    }
}
