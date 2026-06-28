using Microsoft.Extensions.Logging;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies learning moderators when a document is submitted for approval.
/// </summary>
public sealed class DocumentSubmittedForApprovalEventHandler : IDomainEventHandler<DocumentSubmittedForApprovalEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<DocumentSubmittedForApprovalEventHandler> _logger;

    public DocumentSubmittedForApprovalEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<DocumentSubmittedForApprovalEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(DocumentSubmittedForApprovalEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var doc = await _resolver.GetDocumentContextAsync(notification.DocumentId, cancellationToken);
            if (doc is null)
            {
                return;
            }

            var (_, title, courseId) = doc.Value;
            var moderators = await _resolver.GetLearningModeratorUserIdsAsync(courseId, cancellationToken);
            if (moderators.Count == 0)
            {
                return;
            }

            await _dispatcher.SendToManyAsync(
                moderators,
                "Tài liệu mới chờ duyệt",
                $"Tài liệu \"{NotificationMessageHelper.Truncate(title, 60)}\" đang chờ phê duyệt.",
                "document_pending",
                "/mod/learning",
                notification.SubmitterId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling DocumentSubmittedForApprovalEvent for document {DocumentId}",
                notification.DocumentId);
        }
    }
}
