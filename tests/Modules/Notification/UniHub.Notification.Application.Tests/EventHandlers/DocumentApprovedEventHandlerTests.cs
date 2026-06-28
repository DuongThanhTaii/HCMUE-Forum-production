using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using UniHub.Notification.Application.Services;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class DocumentApprovedEventHandlerTests
{
    private readonly DocumentApprovedEventHandler _handler;

    public DocumentApprovedEventHandlerTests()
    {
        var dispatcher = new InAppNotificationDispatcher(
            Substitute.For<INotificationRepository>(),
            Substitute.For<INotificationPusher>(),
            Substitute.For<ILogger<InAppNotificationDispatcher>>());

        _handler = new DocumentApprovedEventHandler(
            dispatcher,
            Substitute.For<INotificationRecipientResolver>(),
            Substitute.For<ILogger<DocumentApprovedEventHandler>>());
    }

    [Fact]
    public async Task Handle_WhenDocumentMissing_CompletesWithoutError()
    {
        var @event = new DocumentApprovedEvent(Guid.NewGuid(), Guid.NewGuid(), "Good work!", DateTime.UtcNow);

        await _handler.Handle(@event, CancellationToken.None);
    }
}
