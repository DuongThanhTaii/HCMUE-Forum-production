using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.Notifications.Events;
using UniHub.Notification.Domain.NotificationTemplates;
using NotificationAggregate = UniHub.Notification.Domain.Notifications.Notification;

namespace UniHub.Notification.Domain.Tests.Notifications;

public class NotificationTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    #region Helper Methods

    private static NotificationContent CreateValidContent()
    {
        return NotificationContent.Create(
            "Test Subject",
            "Test Body",
            "/action",
            "https://icon.url").Value;
    }

    private static NotificationAggregate CreateValidNotification()
    {
        var content = CreateValidContent();
        return NotificationAggregate.Create(
            TestUserId,
            NotificationChannel.Email,
            content).Value;
    }

    private static NotificationTemplate CreateValidTemplate()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Email };
        var template = NotificationTemplate.Create(
            "WelcomeEmail",
            "Welcome Email",
            "Welcome template",
            NotificationCategory.Account,
            channels,
            TestUserId).Value;

        var emailContent = EmailTemplateContent.Create(
            "Welcome {UserName}",
            "Hello {UserName}, welcome to {AppName}!").Value;
        template.UpdateContent(emailContent, null, null, TestUserId);
        template.Activate(TestUserId);

        return template;
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var content = CreateValidContent();

        var result = NotificationAggregate.Create(
            TestUserId,
            NotificationChannel.Email,
            content);

        result.IsSuccess.Should().BeTrue();
        result.Value.RecipientId.Should().Be(TestUserId);
        result.Value.Channel.Should().Be(NotificationChannel.Email);
        result.Value.Content.Should().Be(content);
        result.Value.Status.Should().Be(NotificationStatus.Pending);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.SendAttempts.Should().Be(0);
    }

    [Fact]
    public void Create_WithMetadata_ShouldIncludeMetadata()
    {
        var content = CreateValidContent();
        var metadata = NotificationMetadata.Create(new Dictionary<string, string>
        {
            { "Key1", "Value1" }
        }).Value;

        var result = NotificationAggregate.Create(
            TestUserId,
            NotificationChannel.Email,
            content,
            metadata);

        result.IsSuccess.Should().BeTrue();
        result.Value.Metadata.Should().Be(metadata);
    }

    [Fact]
    public void Create_WithTemplateId_ShouldIncludeTemplateId()
    {
        var content = CreateValidContent();
        var templateId = NotificationTemplateId.CreateUnique();

        var result = NotificationAggregate.Create(
            TestUserId,
            NotificationChannel.Email,
            content,
            null,
            templateId);

        result.IsSuccess.Should().BeTrue();
        result.Value.TemplateId.Should().Be(templateId);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var content = CreateValidContent();

        var result = NotificationAggregate.Create(
            TestUserId,
            NotificationChannel.Email,
            content);

        result.Value.DomainEvents.Should().ContainSingle();
        var domainEvent = result.Value.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationCreatedEvent>();

        var createdEvent = (NotificationCreatedEvent)domainEvent;
        createdEvent.RecipientId.Should().Be(TestUserId);
        createdEvent.Channel.Should().Be(NotificationChannel.Email);
        createdEvent.Subject.Should().Be("Test Subject");
    }

    [Fact]
    public void Create_WithEmptyRecipientId_ShouldReturnFailure()
    {
        var content = CreateValidContent();

        var result = NotificationAggregate.Create(
            Guid.Empty,
            NotificationChannel.Email,
            content);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.RecipientIdEmpty);
    }

    #endregion

    #region CreateFromTemplate Tests

    [Fact]
    public void CreateFromTemplate_WithValidData_ShouldReturnSuccess()
    {
        var template = CreateValidTemplate();
        var variables = new Dictionary<string, string>
        {
            { "UserName", "John Doe" },
            { "AppName", "UniHub" }
        };

        var result = NotificationAggregate.CreateFromTemplate(
            TestUserId,
            template,
            NotificationChannel.Email,
            variables);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Subject.Should().Be("Welcome John Doe");
        result.Value.Content.Body.Should().Be("Hello John Doe, welcome to UniHub!");
        result.Value.TemplateId.Should().Be(template.Id);
        result.Value.Metadata.GetValue("UserName").Should().Be("John Doe");
        result.Value.Metadata.GetValue("AppName").Should().Be("UniHub");
    }

    [Fact]
    public void CreateFromTemplate_WithUnsupportedChannel_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var variables = new Dictionary<string, string>();

        var result = NotificationAggregate.CreateFromTemplate(
            TestUserId,
            template,
            NotificationChannel.Push, // Template only supports Email
            variables);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.UnsupportedChannel");
    }

    [Fact]
    public void CreateFromTemplate_WithMissingContent_ShouldReturnFailure()
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Push };
        var template = NotificationTemplate.Create(
            "TestPush",
            "Test Push",
            null,
            NotificationCategory.Social,
            channels,
            TestUserId).Value;
        // No PushContent configured

        var variables = new Dictionary<string, string>();

        var result = NotificationAggregate.CreateFromTemplate(
            TestUserId,
            template,
            NotificationChannel.Push,
            variables);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.MissingPushContent");
    }

    [Fact]
    public void CreateFromTemplate_WithEmptyRecipientId_ShouldReturnFailure()
    {
        var template = CreateValidTemplate();
        var variables = new Dictionary<string, string>();

        var result = NotificationAggregate.CreateFromTemplate(
            Guid.Empty,
            template,
            NotificationChannel.Email,
            variables);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.RecipientIdEmpty);
    }

    #endregion

    #region MarkAsSent Tests

    [Fact]
    public void MarkAsSent_WhenPending_ShouldSucceed()
    {
        var notification = CreateValidNotification();

        var result = notification.MarkAsSent();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        notification.SendAttempts.Should().Be(1);
    }

    [Fact]
    public void MarkAsSent_ShouldRaiseDomainEvent()
    {
        var notification = CreateValidNotification();
        notification.ClearDomainEvents();

        notification.MarkAsSent();

        notification.DomainEvents.Should().ContainSingle();
        var domainEvent = notification.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationSentEvent>();

        var sentEvent = (NotificationSentEvent)domainEvent;
        sentEvent.NotificationId.Should().Be(notification.Id.Value);
        sentEvent.RecipientId.Should().Be(TestUserId);
    }

    [Fact]
    public void MarkAsSent_WhenAlreadySent_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        var result = notification.MarkAsSent();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.AlreadySent);
    }

    #endregion

    #region MarkAsFailed Tests

    [Fact]
    public void MarkAsFailed_WithValidReason_ShouldSucceed()
    {
        var notification = CreateValidNotification();

        var result = notification.MarkAsFailed("SMTP connection failed");

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.FailureReason.Should().Be("SMTP connection failed");
        notification.SendAttempts.Should().Be(1);
    }

    [Fact]
    public void MarkAsFailed_ShouldRaiseDomainEvent()
    {
        var notification = CreateValidNotification();
        notification.ClearDomainEvents();

        notification.MarkAsFailed("Error occurred");

        notification.DomainEvents.Should().ContainSingle();
        var domainEvent = notification.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationFailedEvent>();

        var failedEvent = (NotificationFailedEvent)domainEvent;
        failedEvent.NotificationId.Should().Be(notification.Id.Value);
        failedEvent.FailureReason.Should().Be("Error occurred");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void MarkAsFailed_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        var notification = CreateValidNotification();

        var result = notification.MarkAsFailed(reason!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.FailureReasonRequired);
    }

    [Fact]
    public void MarkAsFailed_WithReasonTooLong_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        var longReason = new string('R', NotificationAggregate.MaxFailureReasonLength + 1);

        var result = notification.MarkAsFailed(longReason);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.FailureReasonTooLong);
    }

    [Fact]
    public void MarkAsFailed_TrimsWhitespace()
    {
        var notification = CreateValidNotification();

        notification.MarkAsFailed("  Error  ");

        notification.FailureReason.Should().Be("Error");
    }

    #endregion

    #region MarkAsRead Tests

    [Fact]
    public void MarkAsRead_WhenSent_ShouldSucceed()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        var result = notification.MarkAsRead();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Read);
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsRead_WhenPending_ShouldSucceed()
    {
        var notification = CreateValidNotification();

        var result = notification.MarkAsRead();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Read);
    }

    [Fact]
    public void MarkAsRead_ShouldRaiseDomainEvent()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.ClearDomainEvents();

        notification.MarkAsRead();

        notification.DomainEvents.Should().ContainSingle();
        var domainEvent = notification.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationReadEvent>();

        var readEvent = (NotificationReadEvent)domainEvent;
        readEvent.NotificationId.Should().Be(notification.Id.Value);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.MarkAsRead();

        var result = notification.MarkAsRead();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.AlreadyRead);
    }

    [Fact]
    public void MarkAsRead_WhenFailed_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsFailed("Error");

        var result = notification.MarkAsRead();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.CannotMarkFailedAsRead);
    }

    #endregion

    #region Dismiss Tests

    [Fact]
    public void Dismiss_WhenSent_ShouldSucceed()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        var result = notification.Dismiss();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Dismissed);
        notification.DismissedAt.Should().NotBeNull();
        notification.DismissedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dismiss_WhenRead_ShouldSucceed()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.MarkAsRead();

        var result = notification.Dismiss();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Dismissed);
    }

    [Fact]
    public void Dismiss_ShouldRaiseDomainEvent()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.ClearDomainEvents();

        notification.Dismiss();

        notification.DomainEvents.Should().ContainSingle();
        var domainEvent = notification.DomainEvents.First();
        domainEvent.Should().BeOfType<NotificationDismissedEvent>();

        var dismissedEvent = (NotificationDismissedEvent)domainEvent;
        dismissedEvent.NotificationId.Should().Be(notification.Id.Value);
    }

    [Fact]
    public void Dismiss_WhenAlreadyDismissed_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.Dismiss();

        var result = notification.Dismiss();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.AlreadyDismissed);
    }

    [Fact]
    public void Dismiss_WhenFailed_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsFailed("Error");

        var result = notification.Dismiss();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.CannotDismissFailed");
    }

    #endregion

    #region ResetForRetry Tests

    [Fact]
    public void ResetForRetry_WhenFailed_ShouldSucceed()
    {
        var notification = CreateValidNotification();
        notification.MarkAsFailed("Error");

        var result = notification.ResetForRetry();

        result.IsSuccess.Should().BeTrue();
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.FailureReason.Should().BeNull();
    }

    [Fact]
    public void ResetForRetry_WhenNotFailed_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();

        var result = notification.ResetForRetry();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.CanOnlyRetryFailed");
    }

    [Fact]
    public void ResetForRetry_WhenSent_ShouldReturnFailure()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        var result = notification.ResetForRetry();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.CanOnlyRetryFailed");
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public void IsPending_WhenPending_ShouldReturnTrue()
    {
        var notification = CreateValidNotification();

        notification.IsPending().Should().BeTrue();
    }

    [Fact]
    public void IsPending_WhenSent_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        notification.IsPending().Should().BeFalse();
    }

    [Fact]
    public void IsSent_WhenSent_ShouldReturnTrue()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        notification.IsSent().Should().BeTrue();
    }

    [Fact]
    public void IsSent_WhenPending_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();

        notification.IsSent().Should().BeFalse();
    }

    [Fact]
    public void IsFailed_WhenFailed_ShouldReturnTrue()
    {
        var notification = CreateValidNotification();
        notification.MarkAsFailed("Error");

        notification.IsFailed().Should().BeTrue();
    }

    [Fact]
    public void IsFailed_WhenSent_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        notification.IsFailed().Should().BeFalse();
    }

    [Fact]
    public void IsRead_WhenRead_ShouldReturnTrue()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.MarkAsRead();

        notification.IsRead().Should().BeTrue();
    }

    [Fact]
    public void IsRead_WhenSent_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        notification.IsRead().Should().BeFalse();
    }

    [Fact]
    public void IsUnread_WhenSentButNotRead_ShouldReturnTrue()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();

        notification.IsUnread().Should().BeTrue();
    }

    [Fact]
    public void IsUnread_WhenRead_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();
        notification.MarkAsSent();
        notification.MarkAsRead();

        notification.IsUnread().Should().BeFalse();
    }

    [Fact]
    public void IsUnread_WhenPending_ShouldReturnFalse()
    {
        var notification = CreateValidNotification();

        notification.IsUnread().Should().BeFalse();
    }

    #endregion
}
