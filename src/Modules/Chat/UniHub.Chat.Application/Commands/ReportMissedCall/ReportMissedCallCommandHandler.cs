using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.ReportMissedCall;

/// <summary>
/// Saves a MissedCall message and updates conversation LastMessageAt.
/// </summary>
public sealed class ReportMissedCallCommandHandler : ICommandHandler<ReportMissedCallCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    public ReportMissedCallCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Result<Guid>> Handle(
        ReportMissedCallCommand request,
        CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Guid>(new Error("Conversation.NotFound",
                $"Conversation {request.ConversationId} not found"));
        }

        if (!conversation.Participants.Contains(request.CallerId))
        {
            return Result.Failure<Guid>(new Error("Conversation.NotParticipant",
                "Caller is not a participant in this conversation"));
        }

        var msgResult = Message.CreateMissedCall(conversationId, request.CallerId);
        if (msgResult.IsFailure)
        {
            return Result.Failure<Guid>(msgResult.Error);
        }

        var msg = msgResult.Value;
        await _messageRepository.AddAsync(msg, cancellationToken);

        conversation.UpdateLastMessageTime(msg.SentAt);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        return Result.Success(msg.Id.Value);
    }
}
