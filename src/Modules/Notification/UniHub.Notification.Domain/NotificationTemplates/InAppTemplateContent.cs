using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Value object representing in-app notification template content.
/// Supports variable placeholders like {UserName}, {ActionUrl}, etc.
/// </summary>
public sealed class InAppTemplateContent : ValueObject
{
    /// <summary>In-app notification title.</summary>
    public string Title { get; private set; }

    /// <summary>In-app notification body text.</summary>
    public string Body { get; private set; }

    /// <summary>Action URL when notification is clicked (optional).</summary>
    public string? ActionUrl { get; private set; }

    /// <summary>Icon URL for the notification (optional).</summary>
    public string? IconUrl { get; private set; }

    public const int MaxTitleLength = 200;
    public const int MaxBodyLength = 1000;
    public const int MaxActionUrlLength = 2000;
    public const int MaxIconUrlLength = 1000;

    /// <summary>Private constructor for EF Core.</summary>
    private InAppTemplateContent()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    private InAppTemplateContent(
        string title,
        string body,
        string? actionUrl,
        string? iconUrl)
    {
        Title = title;
        Body = body;
        ActionUrl = actionUrl;
        IconUrl = iconUrl;
    }

    /// <summary>
    /// Creates a new InAppTemplateContent value object.
    /// </summary>
    public static Result<InAppTemplateContent> Create(
        string title,
        string body,
        string? actionUrl = null,
        string? iconUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.EmptyTitle", "In-app notification title is required."));

        if (string.IsNullOrWhiteSpace(body))
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.EmptyBody", "In-app notification body is required."));

        // Trim whitespace
        title = title.Trim();
        body = body.Trim();
        actionUrl = actionUrl?.Trim();
        iconUrl = iconUrl?.Trim();

        if (title.Length > MaxTitleLength)
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.TitleTooLong",
                    $"In-app notification title cannot exceed {MaxTitleLength} characters."));

        if (body.Length > MaxBodyLength)
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.BodyTooLong",
                    $"In-app notification body cannot exceed {MaxBodyLength} characters."));

        if (actionUrl?.Length > MaxActionUrlLength)
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.ActionUrlTooLong",
                    $"Action URL cannot exceed {MaxActionUrlLength} characters."));

        if (iconUrl?.Length > MaxIconUrlLength)
            return Result.Failure<InAppTemplateContent>(
                new Error("InAppTemplateContent.IconUrlTooLong",
                    $"Icon URL cannot exceed {MaxIconUrlLength} characters."));

        return Result.Success(new InAppTemplateContent(title, body, actionUrl, iconUrl));
    }

    public override string ToString()
        => $"InApp: {Title}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Body;
        yield return ActionUrl;
        yield return IconUrl;
    }
}
