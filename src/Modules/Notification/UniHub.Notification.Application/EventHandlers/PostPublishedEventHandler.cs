using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies the post author when their post is published (approved).
/// </summary>
public sealed class PostPublishedEventHandler : IDomainEventHandler<PostPublishedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<PostPublishedEventHandler> _logger;

    public PostPublishedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<PostPublishedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(PostPublishedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var postInfo = await _resolver.GetPostAuthorAsync(notification.PostId.Value, cancellationToken);
            if (postInfo is null)
            {
                return;
            }

            var (_, title) = postInfo.Value;
            await _dispatcher.SendAsync(
                notification.AuthorId,
                "Bài viết đã được xuất bản",
                $"Bài \"{NotificationMessageHelper.Truncate(title, 60)}\" đã được duyệt và hiển thị công khai.",
                "post_published",
                $"/forum/{notification.PostId.Value}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PostPublishedEvent for post {PostId}", notification.PostId.Value);
        }
    }
}
