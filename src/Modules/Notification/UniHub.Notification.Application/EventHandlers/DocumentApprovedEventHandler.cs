using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies the document uploader when their document is approved.
/// </summary>
public sealed class DocumentApprovedEventHandler : IDomainEventHandler<DocumentApprovedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<DocumentApprovedEventHandler> _logger;

    public DocumentApprovedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<DocumentApprovedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(DocumentApprovedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var doc = await _resolver.GetDocumentContextAsync(notification.DocumentId, cancellationToken);
            if (doc is null)
            {
                return;
            }

            var (uploaderId, title, _) = doc.Value;
            if (uploaderId == notification.ApproverId)
            {
                return;
            }

            var body = string.IsNullOrWhiteSpace(notification.ApprovalComment)
                ? $"Tài liệu \"{NotificationMessageHelper.Truncate(title, 60)}\" đã được phê duyệt."
                : notification.ApprovalComment;

            await _dispatcher.SendAsync(
                uploaderId,
                "Tài liệu đã được phê duyệt",
                body,
                "document_approved",
                "/learning",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling DocumentApprovedEvent for document {DocumentId}",
                notification.DocumentId);
        }
    }
}
