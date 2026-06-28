using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.ReportCallEnded;

/// <summary>
/// Persists a CallEnded message when a connected call is hung up by either side.
/// </summary>
public sealed record ReportCallEndedCommand(
    Guid ConversationId,
    Guid HangUpUserId,
    int? DurationSeconds = null) : ICommand<Guid>;
