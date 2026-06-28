using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Safety;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.SetConversationMute;

public sealed class SetConversationMuteCommandHandler : ICommandHandler<SetConversationMuteCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationMuteRepository _muteRepository;

    public SetConversationMuteCommandHandler(
        IConversationRepository conversationRepository,
        IConversationMuteRepository muteRepository)
    {
        _conversationRepository = conversationRepository;
        _muteRepository = muteRepository;
    }

    public async Task<Result> Handle(SetConversationMuteCommand request, CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure(new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (!conversation.Participants.Contains(request.UserId))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Not a participant"));
        }

        var mute = ConversationMute.Create(request.UserId, request.ConversationId, request.Muted);
        await _muteRepository.UpsertAsync(mute, cancellationToken);

        return Result.Success();
    }
}
