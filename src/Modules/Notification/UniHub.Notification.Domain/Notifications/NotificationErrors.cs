using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Domain errors for Notification aggregate.
/// </summary>
public static class NotificationErrors
{
    public static readonly Error NotFound = new(
        "Notification.NotFound",
        "Notification was not found.");

    public static readonly Error RecipientIdEmpty = new(
        "Notification.RecipientIdEmpty",
        "Recipient user ID is required.");

    public static readonly Error AlreadySent = new(
        "Notification.AlreadySent",
        "Notification has already been sent.");

    public static readonly Error AlreadyRead = new(
        "Notification.AlreadyRead",
        "Notification has already been read.");

    public static readonly Error AlreadyDismissed = new(
        "Notification.AlreadyDismissed",
        "Notification has already been dismissed.");

    public static readonly Error NotSent = new(
        "Notification.NotSent",
        "Notification has not been sent yet.");

    public static readonly Error CannotMarkFailedAsRead = new(
        "Notification.CannotMarkFailedAsRead",
        "Failed notifications cannot be marked as read.");

    public static readonly Error FailureReasonRequired = new(
        "Notification.FailureReasonRequired",
        "Failure reason is required when marking notification as failed.");

    public static readonly Error FailureReasonTooLong = new(
        "Notification.FailureReasonTooLong",
        $"Failure reason cannot exceed {Notification.MaxFailureReasonLength} characters.");
}
