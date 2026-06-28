using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies forum moderators when a new draft post is created and awaits review.
/// </summary>
public sealed class PostCreatedEventHandler : IDomainEventHandler<PostCreatedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<PostCreatedEventHandler> _logger;

    public PostCreatedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<PostCreatedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(PostCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var postCtx = await _resolver.GetPostContextAsync(notification.PostId.Value, cancellationToken);
            if (postCtx is null)
            {
                return;
            }

            var (_, categoryId, title) = postCtx.Value;
            var moderators = await _resolver.GetForumModeratorUserIdsAsync(categoryId, cancellationToken);
            if (moderators.Count == 0)
            {
                return;
            }

            var actionUrl = "/mod/posts";
            var subject = "Bài viết mới chờ duyệt";
            var body =
                $"Có bài viết nháp mới \"{NotificationMessageHelper.Truncate(title, 60)}\" cần kiểm duyệt.";

            await _dispatcher.SendToManyAsync(
                moderators,
                subject,
                body,
                "post_pending",
                actionUrl,
                notification.AuthorId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PostCreatedEvent for post {PostId}", notification.PostId.Value);
        }
    }
}
