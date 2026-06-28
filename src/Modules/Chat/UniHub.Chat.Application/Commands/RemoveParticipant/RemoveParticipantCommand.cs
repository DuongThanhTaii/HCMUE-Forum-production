using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.RemoveParticipant;

/// <summary>
/// Command to remove a participant from a group conversation
/// </summary>
public sealed record RemoveParticipantCommand(
    Guid ConversationId,
    Guid ParticipantId,
    Guid RemovedBy) : ICommand;
