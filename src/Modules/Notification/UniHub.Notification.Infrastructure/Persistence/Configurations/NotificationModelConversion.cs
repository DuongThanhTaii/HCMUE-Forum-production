using System.Text.Json;
using UniHub.Notification.Domain.NotificationPreferences;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Infrastructure.Persistence.Configurations;

internal static class NotificationModelConversion
{
    public static NotificationId ToNotificationId(Guid value) => NotificationId.Create(value);

    public static NotificationTemplateId ToNotificationTemplateId(Guid value) => NotificationTemplateId.Create(value);

    public static NotificationPreferenceId ToNotificationPreferenceId(Guid value) => NotificationPreferenceId.Create(value);

    public static string ToNotificationContentDb(NotificationContent value)
    {
        var dto = new NotificationContentDto(value.Subject, value.Body, value.ActionUrl, value.IconUrl);
        return JsonSerializer.Serialize(dto);
    }

    public static NotificationContent ToNotificationContentDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<NotificationContentDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize notification content");

        return NotificationContent.Create(dto.Subject, dto.Body, dto.ActionUrl, dto.IconUrl).Value;
    }

    public static string ToNotificationMetadataDb(NotificationMetadata value)
        => JsonSerializer.Serialize(value.Data);

    public static NotificationMetadata ToNotificationMetadataDomain(string raw)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(raw)
            ?? new Dictionary<string, string>();

        return NotificationMetadata.Create(data).Value;
    }

    public static string? ToEmailTemplateContentDb(EmailTemplateContent? value)
    {
        if (value is null)
        {
            return null;
        }

        var dto = new EmailTemplateContentDto(value.Subject, value.Body, value.FromName, value.FromEmail);
        return JsonSerializer.Serialize(dto);
    }

    public static EmailTemplateContent? ToEmailTemplateContentDomain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var dto = JsonSerializer.Deserialize<EmailTemplateContentDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize email template content");

        return EmailTemplateContent.Create(dto.Subject, dto.Body, dto.FromName, dto.FromEmail).Value;
    }

    public static string? ToPushTemplateContentDb(PushTemplateContent? value)
    {
        if (value is null)
        {
            return null;
        }

        var dto = new PushTemplateContentDto(value.Title, value.Body, value.IconUrl, value.BadgeCount);
        return JsonSerializer.Serialize(dto);
    }

    public static PushTemplateContent? ToPushTemplateContentDomain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var dto = JsonSerializer.Deserialize<PushTemplateContentDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize push template content");

        return PushTemplateContent.Create(dto.Title, dto.Body, dto.IconUrl, dto.BadgeCount).Value;
    }

    public static string? ToInAppTemplateContentDb(InAppTemplateContent? value)
    {
        if (value is null)
        {
            return null;
        }

        var dto = new InAppTemplateContentDto(value.Title, value.Body, value.ActionUrl, value.IconUrl);
        return JsonSerializer.Serialize(dto);
    }

    public static InAppTemplateContent? ToInAppTemplateContentDomain(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var dto = JsonSerializer.Deserialize<InAppTemplateContentDto>(raw)
            ?? throw new InvalidOperationException("Unable to deserialize in-app template content");

        return InAppTemplateContent.Create(dto.Title, dto.Body, dto.ActionUrl, dto.IconUrl).Value;
    }

    public static string ToChannelListDb(List<NotificationChannel> values)
        => JsonSerializer.Serialize(values.Select(v => (int)v).ToList());

    public static List<NotificationChannel> ToChannelListDomain(string raw)
    {
        var data = JsonSerializer.Deserialize<List<int>>(raw) ?? new List<int>();
        return data.Select(v => (NotificationChannel)v).ToList();
    }

    public static string ToTemplateVariableListDb(List<TemplateVariable> values)
    {
        var dto = values.Select(v => new TemplateVariableDto(v.Name, v.Description, v.ExampleValue)).ToList();
        return JsonSerializer.Serialize(dto);
    }

    public static List<TemplateVariable> ToTemplateVariableListDomain(string raw)
    {
        var dto = JsonSerializer.Deserialize<List<TemplateVariableDto>>(raw) ?? new List<TemplateVariableDto>();
        return dto.Select(v => TemplateVariable.Create(v.Name, v.Description, v.ExampleValue).Value).ToList();
    }

    private sealed record NotificationContentDto(string Subject, string Body, string? ActionUrl, string? IconUrl);

    private sealed record EmailTemplateContentDto(string Subject, string Body, string? FromName, string? FromEmail);

    private sealed record PushTemplateContentDto(string Title, string Body, string? IconUrl, int? BadgeCount);

    private sealed record InAppTemplateContentDto(string Title, string Body, string? ActionUrl, string? IconUrl);

    private sealed record TemplateVariableDto(string Name, string Description, string? ExampleValue);
}
