using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.Chat.Domain.Safety;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.ReportMessage;

public sealed class ReportMessageCommandHandler : ICommandHandler<ReportMessageCommand>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatMessageReportRepository _reportRepository;

    public ReportMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IChatMessageReportRepository reportRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _reportRepository = reportRepository;
    }

    public async Task<Result> Handle(ReportMessageCommand request, CancellationToken cancellationToken)
    {
        var messageId = MessageId.Create(request.MessageId);
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(new Error("Message.NotFound", "Message not found"));
        }

        var conversation = await _conversationRepository.GetByIdAsync(
            message.ConversationId,
            cancellationToken);

        if (conversation is null || !conversation.Participants.Contains(request.ReporterId))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Not a participant"));
        }

        if (await _reportRepository.ExistsAsync(request.ReporterId, request.MessageId, cancellationToken))
        {
            return Result.Failure(new Error("ChatReport.AlreadyReported", "You already reported this message"));
        }

        var reportResult = ChatMessageReport.Create(
            request.MessageId,
            message.ConversationId.Value,
            request.ReporterId,
            request.Reason,
            request.Description);

        if (reportResult.IsFailure)
        {
            return Result.Failure(reportResult.Error);
        }

        await _reportRepository.AddAsync(reportResult.Value, cancellationToken);

        return Result.Success();
    }
}
