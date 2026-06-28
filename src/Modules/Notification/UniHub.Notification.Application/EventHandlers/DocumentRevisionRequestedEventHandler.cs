using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies the uploader when a moderator requests revisions on their document.
/// </summary>
public sealed class DocumentRevisionRequestedEventHandler : IDomainEventHandler<DocumentRevisionRequestedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<DocumentRevisionRequestedEventHandler> _logger;

    public DocumentRevisionRequestedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<DocumentRevisionRequestedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(DocumentRevisionRequestedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var doc = await _resolver.GetDocumentContextAsync(notification.DocumentId, cancellationToken);
            if (doc is null)
            {
                return;
            }

            var (uploaderId, title, _) = doc.Value;
            var noteText = notification.RevisionNotes ?? notification.RevisionReason;
            var note = NotificationMessageHelper.Truncate(noteText, 200);

            await _dispatcher.SendAsync(
                uploaderId,
                "Yêu cầu chỉnh sửa tài liệu",
                $"Tài liệu \"{NotificationMessageHelper.Truncate(title, 60)}\" cần chỉnh sửa: {note}",
                "document_revision",
                "/learning",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling DocumentRevisionRequestedEvent for document {DocumentId}",
                notification.DocumentId);
        }
    }
}
