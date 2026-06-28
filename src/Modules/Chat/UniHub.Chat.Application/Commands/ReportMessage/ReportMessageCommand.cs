using UniHub.Chat.Domain.Safety;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.ReportMessage;

public sealed record ReportMessageCommand(
    Guid ReporterId,
    Guid MessageId,
    ChatMessageReportReason Reason,
    string? Description) : ICommand;
