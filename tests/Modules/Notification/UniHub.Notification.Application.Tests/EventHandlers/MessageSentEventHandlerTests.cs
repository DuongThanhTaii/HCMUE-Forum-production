using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Chat.Domain.Messages;
using UniHub.Chat.Domain.Messages.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using UniHub.Notification.Application.Services;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class MessageSentEventHandlerTests
{
    private readonly MessageSentEventHandler _handler;

    public MessageSentEventHandlerTests()
    {
        var dispatcher = new InAppNotificationDispatcher(
            Substitute.For<INotificationRepository>(),
            Substitute.For<INotificationPusher>(),
            Substitute.For<ILogger<InAppNotificationDispatcher>>());

        _handler = new MessageSentEventHandler(
            dispatcher,
            Substitute.For<INotificationRecipientResolver>(),
            Substitute.For<ILogger<MessageSentEventHandler>>());
    }

    [Fact]
    public async Task Handle_WhenNoRecipients_CompletesWithoutError()
    {
        var @event = new MessageSentEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            MessageType.Text,
            "Hello!",
            DateTime.UtcNow);

        await _handler.Handle(@event, CancellationToken.None);
    }
}
