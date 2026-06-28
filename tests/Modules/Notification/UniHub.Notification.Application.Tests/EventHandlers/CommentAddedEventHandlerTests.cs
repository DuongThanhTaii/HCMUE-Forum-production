using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Posts;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using UniHub.Notification.Application.Services;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class CommentAddedEventHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPusher _pusher;
    private readonly INotificationRecipientResolver _resolver;
    private readonly CommentAddedEventHandler _handler;

    public CommentAddedEventHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _pusher = Substitute.For<INotificationPusher>();
        _resolver = Substitute.For<INotificationRecipientResolver>();

        var dispatcher = new InAppNotificationDispatcher(
            _notificationRepository,
            _pusher,
            Substitute.For<ILogger<InAppNotificationDispatcher>>());

        _handler = new CommentAddedEventHandler(
            dispatcher,
            _resolver,
            Substitute.For<ILogger<CommentAddedEventHandler>>());
    }

    [Fact]
    public async Task Handle_WhenPostAuthorDiffers_ShouldNotifyPostAuthor()
    {
        var commentId = CommentId.CreateUnique();
        var postId = PostId.CreateUnique();
        var authorId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var @event = new CommentAddedEvent(commentId, postId, authorId, null);

        _resolver.GetPostAuthorAsync(postId.Value, Arg.Any<CancellationToken>())
            .Returns((postAuthorId, "Test post"));

        await _handler.Handle(@event, CancellationToken.None);

        await _notificationRepository.Received(1).AddAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
        await _pusher.Received(1).PushAsync(
            postAuthorId,
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            "comment",
            Arg.Any<DateTime>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }
}
