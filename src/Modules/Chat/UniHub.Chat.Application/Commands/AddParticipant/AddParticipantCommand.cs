using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.AddParticipant;

/// <summary>
/// Command to add a participant to a group conversation
/// </summary>
public sealed record AddParticipantCommand(
    Guid ConversationId,
    Guid ParticipantId,
    Guid AddedBy) : ICommand;
