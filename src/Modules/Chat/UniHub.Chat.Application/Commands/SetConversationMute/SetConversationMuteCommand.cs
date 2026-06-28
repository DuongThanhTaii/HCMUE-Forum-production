using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.SetConversationMute;

public sealed record SetConversationMuteCommand(
    Guid UserId,
    Guid ConversationId,
    bool Muted) : ICommand;
