using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Value object representing push notification template content.
/// Supports variable placeholders like {UserName}, {ActionUrl}, etc.
/// </summary>
public sealed class PushTemplateContent : ValueObject
{
    /// <summary>Push notification title.</summary>
    public string Title { get; private set; }

    /// <summary>Push notification body text.</summary>
    public string Body { get; private set; }

    /// <summary>Icon URL for the notification (optional).</summary>
    public string? IconUrl { get; private set; }

    /// <summary>Badge count to display (optional).</summary>
    public int? BadgeCount { get; private set; }

    public const int MaxTitleLength = 100;
    public const int MaxBodyLength = 500;
    public const int MaxIconUrlLength = 1000;

    /// <summary>Private constructor for EF Core.</summary>
    private PushTemplateContent()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    private PushTemplateContent(
        string title,
        string body,
        string? iconUrl,
        int? badgeCount)
    {
        Title = title;
        Body = body;
        IconUrl = iconUrl;
        BadgeCount = badgeCount;
    }

    /// <summary>
    /// Creates a new PushTemplateContent value object.
    /// </summary>
    public static Result<PushTemplateContent> Create(
        string title,
        string body,
        string? iconUrl = null,
        int? badgeCount = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.EmptyTitle", "Push notification title is required."));

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.EmptyBody", "Push notification body is required."));

        // Trim whitespace
        title = title.Trim();
        body = body.Trim();
        iconUrl = iconUrl?.Trim();

        if (title.Length > MaxTitleLength)
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.TitleTooLong",
                    $"Push notification title cannot exceed {MaxTitleLength} characters."));

        if (body.Length > MaxBodyLength)
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.BodyTooLong",
                    $"Push notification body cannot exceed {MaxBodyLength} characters."));

        if (iconUrl?.Length > MaxIconUrlLength)
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.IconUrlTooLong",
                    $"Icon URL cannot exceed {MaxIconUrlLength} characters."));

        if (badgeCount.HasValue && badgeCount.Value < 0)
            return Result.Failure<PushTemplateContent>(
                new Error("PushTemplateContent.InvalidBadgeCount", "Badge count cannot be negative."));

        return Result.Success(new PushTemplateContent(title, body, iconUrl, badgeCount));
    }

    public override string ToString()
        => $"Push: {Title}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Body;
        yield return IconUrl;
        yield return BadgeCount;
    }
}
