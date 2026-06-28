using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies the uploader when their document is rejected.
/// </summary>
public sealed class DocumentRejectedEventHandler : IDomainEventHandler<DocumentRejectedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<DocumentRejectedEventHandler> _logger;

    public DocumentRejectedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<DocumentRejectedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(DocumentRejectedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var doc = await _resolver.GetDocumentContextAsync(notification.DocumentId, cancellationToken);
            if (doc is null)
            {
                return;
            }

            var (uploaderId, title, _) = doc.Value;
            var reason = NotificationMessageHelper.Truncate(notification.RejectionReason, 200);

            await _dispatcher.SendAsync(
                uploaderId,
                "Tài liệu bị từ chối",
                $"Tài liệu \"{NotificationMessageHelper.Truncate(title, 60)}\" bị từ chối. Lý do: {reason}",
                "document_rejected",
                "/learning",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling DocumentRejectedEvent for document {DocumentId}",
                notification.DocumentId);
        }
    }
}
