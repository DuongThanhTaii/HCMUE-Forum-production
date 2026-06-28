using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Reports;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies moderators when a user submits a report.
/// </summary>
public sealed class ReportSubmittedEventHandler : IDomainEventHandler<ReportSubmittedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<ReportSubmittedEventHandler> _logger;

    public ReportSubmittedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<ReportSubmittedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(ReportSubmittedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var moderators = await _resolver.GetAdminAndModeratorUserIdsAsync(cancellationToken);
            if (moderators.Count == 0)
            {
                return;
            }

            var itemLabel = notification.ReportedItemType == ReportedItemType.Post ? "bài viết" : "bình luận";
            var subject = "Báo cáo vi phạm mới";
            var body = $"Có báo cáo mới về {itemLabel} (lý do: {notification.Reason}).";

            await _dispatcher.SendToManyAsync(
                moderators,
                subject,
                body,
                "report",
                "/mod/reports",
                notification.ReporterId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ReportSubmittedEvent for report {ReportId}", notification.ReportId);
        }
    }
}
