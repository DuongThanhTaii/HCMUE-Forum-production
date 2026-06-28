using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.ReportMissedCall;

/// <summary>
/// Persists a missed-call notification as a MissedCall message in the conversation.
/// Invoked by the caller (via SignalR hub) after ending the call without the callee answering.
/// </summary>
public sealed record ReportMissedCallCommand(
    Guid ConversationId,
    Guid CallerId) : ICommand<Guid>;
