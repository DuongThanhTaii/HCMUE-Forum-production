using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies post author on new comments and parent-comment author on replies.
/// </summary>
public sealed class CommentAddedEventHandler : IDomainEventHandler<CommentAddedEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<CommentAddedEventHandler> _logger;

    public CommentAddedEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<CommentAddedEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var postInfo = await _resolver.GetPostAuthorAsync(notification.PostId.Value, cancellationToken);
            if (postInfo is null)
            {
                return;
            }

            var (postAuthorId, postTitle) = postInfo.Value;
            var postUrl = $"/forum/{notification.PostId.Value}#comment-{notification.CommentId.Value}";

            if (postAuthorId != notification.AuthorId)
            {
                await _dispatcher.SendAsync(
                    postAuthorId,
                    "Bình luận mới trong bài viết của bạn",
                    $"Có bình luận mới trên bài \"{NotificationMessageHelper.Truncate(postTitle, 60)}\".",
                    "comment",
                    postUrl,
                    cancellationToken);
            }

            if (notification.ParentCommentId is not null)
            {
                var parentCtx = await _resolver.GetCommentContextAsync(
                    notification.ParentCommentId.Value,
                    cancellationToken);

                if (parentCtx is not null)
                {
                    var (parentAuthorId, _, _) = parentCtx.Value;
                    if (parentAuthorId != notification.AuthorId && parentAuthorId != postAuthorId)
                    {
                        await _dispatcher.SendAsync(
                            parentAuthorId,
                            "Phản hồi bình luận của bạn",
                            $"Ai đó đã trả lời bình luận của bạn trong bài \"{NotificationMessageHelper.Truncate(postTitle, 60)}\".",
                            "comment_reply",
                            postUrl,
                            cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CommentAddedEvent for comment {CommentId}", notification.CommentId.Value);
        }
    }
}
