using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Posts;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using UniHub.Notification.Application.Services;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class PostCreatedEventHandlerTests
{
    private readonly PostCreatedEventHandler _handler;

    public PostCreatedEventHandlerTests()
    {
        var dispatcher = new InAppNotificationDispatcher(
            Substitute.For<INotificationRepository>(),
            Substitute.For<INotificationPusher>(),
            Substitute.For<ILogger<InAppNotificationDispatcher>>());

        _handler = new PostCreatedEventHandler(
            dispatcher,
            Substitute.For<INotificationRecipientResolver>(),
            Substitute.For<ILogger<PostCreatedEventHandler>>());
    }

    [Fact]
    public async Task Handle_WhenPostMissing_CompletesWithoutError()
    {
        var postId = PostId.CreateUnique();
        var @event = new PostCreatedEvent(postId, Guid.NewGuid(), PostType.Discussion);

        await _handler.Handle(@event, CancellationToken.None);
    }
}
