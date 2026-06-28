using Microsoft.Extensions.Logging;
using UniHub.Forum.Domain.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Services;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Notifies the comment author when their comment is accepted as the answer.
/// </summary>
public sealed class CommentAcceptedAsAnswerEventHandler : IDomainEventHandler<CommentAcceptedAsAnswerEvent>
{
    private readonly InAppNotificationDispatcher _dispatcher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly ILogger<CommentAcceptedAsAnswerEventHandler> _logger;

    public CommentAcceptedAsAnswerEventHandler(
        InAppNotificationDispatcher dispatcher,
        INotificationRecipientResolver resolver,
        ILogger<CommentAcceptedAsAnswerEventHandler> logger)
    {
        _dispatcher = dispatcher;
        _resolver = resolver;
        _logger = logger;
    }

    public async Task Handle(CommentAcceptedAsAnswerEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var postInfo = await _resolver.GetPostAuthorAsync(notification.PostId.Value, cancellationToken);
            var title = postInfo?.Title ?? "Bài viết";

            await _dispatcher.SendAsync(
                notification.AuthorId,
                "Bình luận được chọn làm câu trả lời",
                $"Bình luận của bạn đã được chấp nhận làm câu trả lời hay nhất trong \"{NotificationMessageHelper.Truncate(title, 60)}\".",
                "comment_answer",
                $"/forum/{notification.PostId.Value}#comment-{notification.CommentId.Value}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling CommentAcceptedAsAnswerEvent for comment {CommentId}",
                notification.CommentId.Value);
        }
    }
}
