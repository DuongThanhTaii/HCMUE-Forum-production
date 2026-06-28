using UniHub.Notification.Domain.Notifications.Events;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Notification Aggregate Root - represents an actual notification sent to a user.
/// Can be created from a template or standalone.
/// Lifecycle: Pending → Sent/Failed → Read/Dismissed.
/// </summary>
public sealed class Notification : AggregateRoot<NotificationId>
{
    #region Constants

    public const int MaxFailureReasonLength = 500;

    #endregion

    #region Properties

    /// <summary>The user who will receive this notification.</summary>
    public Guid RecipientId { get; private set; }

    /// <summary>The template used to create this notification (optional).</summary>
    public NotificationTemplateId? TemplateId { get; private set; }

    /// <summary>The delivery channel for this notification.</summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>Current status of the notification.</summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>Notification content (subject, body, action URL, icon).</summary>
    public NotificationContent Content { get; private set; }

    /// <summary>Metadata for variable substitution and tracking.</summary>
    public NotificationMetadata Metadata { get; private set; }

    /// <summary>When the notification was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>When the notification was sent (if sent).</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>When the notification was read (if read).</summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>When the notification was dismissed (if dismissed).</summary>
    public DateTime? DismissedAt { get; private set; }

    /// <summary>Reason for failure (if failed).</summary>
    public string? FailureReason { get; private set; }

    /// <summary>Number of send attempts.</summary>
    public int SendAttempts { get; private set; }

    #endregion

    #region Constructors

    /// <summary>Private constructor for EF Core.</summary>
    private Notification()
    {
        Content = null!;
        Metadata = null!;
    }

    private Notification(
        NotificationId id,
        Guid recipientId,
        NotificationTemplateId? templateId,
        NotificationChannel channel,
        NotificationContent content,
        NotificationMetadata metadata)
        : base(id)
    {
        RecipientId = recipientId;
        TemplateId = templateId;
        Channel = channel;
        Content = content;
        Metadata = metadata;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        SendAttempts = 0;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new notification for a recipient.
    /// </summary>
    public static Result<Notification> Create(
        Guid recipientId,
        NotificationChannel channel,
        NotificationContent content,
        NotificationMetadata? metadata = null,
        NotificationTemplateId? templateId = null)
    {
        if (recipientId == Guid.Empty)
            return Result.Failure<Notification>(NotificationErrors.RecipientIdEmpty);

        metadata ??= NotificationMetadata.Empty();

        var notification = new Notification(
            NotificationId.CreateUnique(),
            recipientId,
            templateId,
            channel,
            content,
            metadata);

        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id.Value,
            recipientId,
            channel,
            content.Subject,
            notification.CreatedAt));

        return Result.Success(notification);
    }

    /// <summary>
    /// Creates a notification from a template with variable substitution.
    /// </summary>
    public static Result<Notification> CreateFromTemplate(
        Guid recipientId,
        NotificationTemplate template,
        NotificationChannel channel,
        Dictionary<string, string> variables)
    {
        if (recipientId == Guid.Empty)
            return Result.Failure<Notification>(NotificationErrors.RecipientIdEmpty);

        if (!template.SupportsChannel(channel))
            return Result.Failure<Notification>(new Error(
                "Notification.UnsupportedChannel",
                $"Template does not support {channel} channel."));

        // Get the appropriate template content based on channel
        string subject;
        string body;
        string? actionUrl = null;
        string? iconUrl = null;

        switch (channel)
        {
            case NotificationChannel.Email:
                if (template.EmailContent == null)
                    return Result.Failure<Notification>(new Error(
                        "Notification.MissingEmailContent",
                        "Template does not have email content configured."));
                subject = SubstituteVariables(template.EmailContent.Subject, variables);
                body = SubstituteVariables(template.EmailContent.Body, variables);
                break;

            case NotificationChannel.Push:
                if (template.PushContent == null)
                    return Result.Failure<Notification>(new Error(
                        "Notification.MissingPushContent",
                        "Template does not have push content configured."));
                subject = SubstituteVariables(template.PushContent.Title, variables);
                body = SubstituteVariables(template.PushContent.Body, variables);
                iconUrl = template.PushContent.IconUrl;
                break;

            case NotificationChannel.InApp:
                if (template.InAppContent == null)
                    return Result.Failure<Notification>(new Error(
                        "Notification.MissingInAppContent",
                        "Template does not have in-app content configured."));
                subject = SubstituteVariables(template.InAppContent.Title, variables);
                body = SubstituteVariables(template.InAppContent.Body, variables);
                actionUrl = template.InAppContent.ActionUrl;
                iconUrl = template.InAppContent.IconUrl;
                break;

            default:
                return Result.Failure<Notification>(new Error(
                    "Notification.InvalidChannel",
                    "Invalid notification channel."));
        }

        var contentResult = NotificationContent.Create(subject, body, actionUrl, iconUrl);
        if (contentResult.IsFailure)
            return Result.Failure<Notification>(contentResult.Error);

        var metadataResult = NotificationMetadata.Create(variables);
        if (metadataResult.IsFailure)
            return Result.Failure<Notification>(metadataResult.Error);

        var notification = new Notification(
            NotificationId.CreateUnique(),
            recipientId,
            template.Id,
            channel,
            contentResult.Value,
            metadataResult.Value);

        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id.Value,
            recipientId,
            channel,
            subject,
            notification.CreatedAt));

        return Result.Success(notification);
    }

    /// <summary>
    /// Substitutes variables in a template string with actual values.
    /// </summary>
    private static string SubstituteVariables(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }

    #endregion

    #region Behavior Methods

    /// <summary>
    /// Marks the notification as sent.
    /// </summary>
    public Result MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            return Result.Failure(NotificationErrors.AlreadySent);

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        SendAttempts++;

        AddDomainEvent(new NotificationSentEvent(
            Id.Value,
            RecipientId,
            SentAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Marks the notification as failed with a reason.
    /// </summary>
    public Result MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            return Result.Failure(NotificationErrors.FailureReasonRequired);

        failureReason = failureReason.Trim();
        if (failureReason.Length > MaxFailureReasonLength)
            return Result.Failure(NotificationErrors.FailureReasonTooLong);

        Status = NotificationStatus.Failed;
        FailureReason = failureReason;
        SendAttempts++;

        AddDomainEvent(new NotificationFailedEvent(
            Id.Value,
            RecipientId,
            failureReason,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Marks the notification as read by the recipient.
    /// </summary>
    public Result MarkAsRead()
    {
        if (Status == NotificationStatus.Failed)
            return Result.Failure(NotificationErrors.CannotMarkFailedAsRead);

        if (Status == NotificationStatus.Read)
            return Result.Failure(NotificationErrors.AlreadyRead);

        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationReadEvent(
            Id.Value,
            RecipientId,
            ReadAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Dismisses the notification.
    /// </summary>
    public Result Dismiss()
    {
        if (Status == NotificationStatus.Dismissed)
            return Result.Failure(NotificationErrors.AlreadyDismissed);

        if (Status == NotificationStatus.Failed)
            return Result.Failure(new Error(
                "Notification.CannotDismissFailed",
                "Failed notifications cannot be dismissed."));

        Status = NotificationStatus.Dismissed;
        DismissedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationDismissedEvent(
            Id.Value,
            RecipientId,
            DismissedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Resets the notification to pending for retry.
    /// </summary>
    public Result ResetForRetry()
    {
        if (Status != NotificationStatus.Failed)
            return Result.Failure(new Error(
                "Notification.CanOnlyRetryFailed",
                "Only failed notifications can be reset for retry."));

        Status = NotificationStatus.Pending;
        FailureReason = null;

        return Result.Success();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the notification is pending.
    /// </summary>
    public bool IsPending() => Status == NotificationStatus.Pending;

    /// <summary>
    /// Checks if the notification has been sent.
    /// </summary>
    public bool IsSent() => Status == NotificationStatus.Sent;

    /// <summary>
    /// Checks if the notification has failed.
    /// </summary>
    public bool IsFailed() => Status == NotificationStatus.Failed;

    /// <summary>
    /// Checks if the notification has been read.
    /// </summary>
    public bool IsRead() => Status == NotificationStatus.Read;

    /// <summary>
    /// Checks if the notification is unread (sent but not read).
    /// </summary>
    public bool IsUnread() => Status == NotificationStatus.Sent && ReadAt == null;

    #endregion
}
