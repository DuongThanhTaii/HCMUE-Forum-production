using Microsoft.Extensions.Logging;
using UniHub.Chat.Domain.Messages.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Sends in-app notifications to conversation participants when a new message arrives.
/// </summary>
public sealed class MessageSentEventHandler : IDomainEventHandler<MessageSentEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<MessageSentEventHandler> _logger;

    public MessageSentEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<MessageSentEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var recipients = await _resolver.GetConversationParticipantIdsExceptAsync(
                notification.ConversationId,
                notification.SenderId,
                cancellationToken);

            if (recipients.Count == 0)
            {
                return;
            }

            var preview = NotificationMessageHelper.Truncate(notification.Content, 80);
            var actionUrl = $"/chat?conversation={notification.ConversationId}";

            await _dispatcher.SendToManyAsync(
                recipients,
                "Tin nhắn mới",
                string.IsNullOrWhiteSpace(preview) ? "Bạn có tin nhắn mới." : preview,
                "chat_message",
                actionUrl,
                notification.SenderId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MessageSentEvent for message {MessageId}", notification.MessageId);
        }
    }
}
