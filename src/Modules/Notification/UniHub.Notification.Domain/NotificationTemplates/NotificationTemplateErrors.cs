using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Domain errors for NotificationTemplate aggregate.
/// </summary>
public static class NotificationTemplateErrors
{
    public static readonly Error NotFound = new(
        "NotificationTemplate.NotFound",
        "Notification template was not found.");

    public static readonly Error NameEmpty = new(
        "NotificationTemplate.NameEmpty",
        "Template name is required.");

    public static readonly Error NameTooLong = new(
        "NotificationTemplate.NameTooLong",
        $"Template name cannot exceed {NotificationTemplate.MaxNameLength} characters.");

    public static readonly Error DisplayNameEmpty = new(
        "NotificationTemplate.DisplayNameEmpty",
        "Template display name is required.");

    public static readonly Error DisplayNameTooLong = new(
        "NotificationTemplate.DisplayNameTooLong",
        $"Template display name cannot exceed {NotificationTemplate.MaxDisplayNameLength} characters.");

    public static readonly Error DescriptionTooLong = new(
        "NotificationTemplate.DescriptionTooLong",
        $"Template description cannot exceed {NotificationTemplate.MaxDescriptionLength} characters.");

    public static readonly Error NoChannels = new(
        "NotificationTemplate.NoChannels",
        "At least one notification channel must be specified.");

    public static readonly Error EmailChannelWithoutContent = new(
        "NotificationTemplate.EmailChannelWithoutContent",
        "Email channel requires email template content.");

    public static readonly Error PushChannelWithoutContent = new(
        "NotificationTemplate.PushChannelWithoutContent",
        "Push channel requires push template content.");

    public static readonly Error InAppChannelWithoutContent = new(
        "NotificationTemplate.InAppChannelWithoutContent",
        "InApp channel requires in-app template content.");

    public static readonly Error AlreadyActive = new(
        "NotificationTemplate.AlreadyActive",
        "Template is already active.");

    public static readonly Error AlreadyArchived = new(
        "NotificationTemplate.AlreadyArchived",
        "Template is already archived.");

    public static readonly Error CannotActivateDraft = new(
        "NotificationTemplate.CannotActivateDraft",
        "Draft template must have content configured before activation.");

    public static readonly Error TooManyVariables = new(
        "NotificationTemplate.TooManyVariables",
        $"Cannot exceed {NotificationTemplate.MaxVariables} variables.");

    public static readonly Error DuplicateVariable = new(
        "NotificationTemplate.DuplicateVariable",
        "Variable with this name already exists.");

    public static readonly Error VariableNotFound = new(
        "NotificationTemplate.VariableNotFound",
        "Variable was not found in template.");
}
