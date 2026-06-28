using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Value object representing the content of a notification.
/// </summary>
public sealed class NotificationContent : ValueObject
{
    /// <summary>Notification subject or title.</summary>
    public string Subject { get; private set; }

    /// <summary>Notification body or message.</summary>
    public string Body { get; private set; }

    /// <summary>Action URL when notification is clicked (optional).</summary>
    public string? ActionUrl { get; private set; }

    /// <summary>Icon URL for the notification (optional).</summary>
    public string? IconUrl { get; private set; }

    public const int MaxSubjectLength = 200;
    public const int MaxBodyLength = 2000;
    public const int MaxActionUrlLength = 2000;
    public const int MaxIconUrlLength = 1000;

    /// <summary>Private constructor for EF Core.</summary>
    private NotificationContent()
    {
        Subject = string.Empty;
        Body = string.Empty;
    }

    private NotificationContent(
        string subject,
        string body,
        string? actionUrl,
        string? iconUrl)
    {
        Subject = subject;
        Body = body;
        ActionUrl = actionUrl;
        IconUrl = iconUrl;
    }

    /// <summary>
    /// Creates a new NotificationContent value object.
    /// </summary>
    public static Result<NotificationContent> Create(
        string subject,
        string body,
        string? actionUrl = null,
        string? iconUrl = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.EmptySubject", "Notification subject is required."));

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.EmptyBody", "Notification body is required."));

        // Trim whitespace
        subject = subject.Trim();
        body = body.Trim();
        actionUrl = actionUrl?.Trim();
        iconUrl = iconUrl?.Trim();

        if (subject.Length > MaxSubjectLength)
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.SubjectTooLong",
                    $"Notification subject cannot exceed {MaxSubjectLength} characters."));

        if (body.Length > MaxBodyLength)
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.BodyTooLong",
                    $"Notification body cannot exceed {MaxBodyLength} characters."));

        if (actionUrl?.Length > MaxActionUrlLength)
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.ActionUrlTooLong",
                    $"Action URL cannot exceed {MaxActionUrlLength} characters."));

        if (iconUrl?.Length > MaxIconUrlLength)
            return Result.Failure<NotificationContent>(
                new Error("NotificationContent.IconUrlTooLong",
                    $"Icon URL cannot exceed {MaxIconUrlLength} characters."));

        return Result.Success(new NotificationContent(subject, body, actionUrl, iconUrl));
    }

    public override string ToString()
        => $"{Subject}: {Body.Substring(0, Math.Min(50, Body.Length))}...";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Subject;
        yield return Body;
        yield return ActionUrl;
        yield return IconUrl;
    }
}
