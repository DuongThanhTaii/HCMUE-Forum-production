using UniHub.Notification.Domain.NotificationTemplates.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// NotificationTemplate Aggregate Root - represents a reusable template for sending notifications.
/// Supports Email, Push, and In-App channels with dynamic variable substitution.
/// Lifecycle: Draft → Active → Archived.
/// </summary>
public sealed class NotificationTemplate : AggregateRoot<NotificationTemplateId>
{
    #region Constants

    public const int MaxNameLength = 100;
    public const int MaxDisplayNameLength = 200;
    public const int MaxDescriptionLength = 1000;
    public const int MaxVariables = 50;

    #endregion

    #region Properties

    /// <summary>Unique template name identifier (e.g., "WelcomeEmail", "JobPostedAlert").</summary>
    public string Name { get; private set; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; private set; }

    /// <summary>Template description (optional).</summary>
    public string? Description { get; private set; }

    /// <summary>Template category for organization.</summary>
    public NotificationCategory Category { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public NotificationTemplateStatus Status { get; private set; }

    /// <summary>Email template content (if Email channel is enabled).</summary>
    public EmailTemplateContent? EmailContent { get; private set; }

    /// <summary>Push notification template content (if Push channel is enabled).</summary>
    public PushTemplateContent? PushContent { get; private set; }

    /// <summary>In-app notification template content (if InApp channel is enabled).</summary>
    public InAppTemplateContent? InAppContent { get; private set; }

    /// <summary>Who created this template.</summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>When the template was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Who last updated the template (if updated).</summary>
    public Guid? UpdatedBy { get; private set; }

    /// <summary>When the template was last updated (if updated).</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>When the template was activated (if active).</summary>
    public DateTime? ActivatedAt { get; private set; }

    /// <summary>Who activated the template.</summary>
    public Guid? ActivatedBy { get; private set; }

    #endregion

    #region Collections

    private readonly List<NotificationChannel> _channels = new();
    /// <summary>Enabled notification channels for this template.</summary>
    public IReadOnlyList<NotificationChannel> Channels => _channels.AsReadOnly();

    private readonly List<TemplateVariable> _variables = new();
    /// <summary>Template variables that can be substituted when sending notifications.</summary>
    public IReadOnlyList<TemplateVariable> Variables => _variables.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>Private constructor for EF Core.</summary>
    private NotificationTemplate()
    {
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    private NotificationTemplate(
        NotificationTemplateId id,
        string name,
        string displayName,
        string? description,
        NotificationCategory category,
        List<NotificationChannel> channels,
        Guid createdBy)
        : base(id)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Category = category;
        _channels = channels;
        Status = NotificationTemplateStatus.Draft;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Factory Method

    /// <summary>
    /// Creates a new notification template in Draft status.
    /// </summary>
    public static Result<NotificationTemplate> Create(
        string name,
        string displayName,
        string? description,
        NotificationCategory category,
        List<NotificationChannel> channels,
        Guid createdBy)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.NameEmpty);

        name = name.Trim();
        if (name.Length > MaxNameLength)
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.NameTooLong);

        // Validate display name
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.DisplayNameEmpty);

        displayName = displayName.Trim();
        if (displayName.Length > MaxDisplayNameLength)
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.DisplayNameTooLong);

        // Validate description
        description = description?.Trim();
        if (description?.Length > MaxDescriptionLength)
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.DescriptionTooLong);

        // Validate channels
        if (channels == null || channels.Count == 0)
            return Result.Failure<NotificationTemplate>(NotificationTemplateErrors.NoChannels);

        var template = new NotificationTemplate(
            NotificationTemplateId.CreateUnique(),
            name,
            displayName,
            description,
            category,
            channels,
            createdBy);

        template.AddDomainEvent(new NotificationTemplateCreatedEvent(
            template.Id.Value,
            template.Name,
            template.DisplayName,
            template.Category,
            createdBy,
            template.CreatedAt));

        return Result.Success(template);
    }

    #endregion

    #region Behavior Methods

    /// <summary>
    /// Activates the template, making it available for use.
    /// Template must have content configured for all enabled channels.
    /// </summary>
    public Result Activate(Guid activatedBy)
    {
        if (Status == NotificationTemplateStatus.Active)
            return Result.Failure(NotificationTemplateErrors.AlreadyActive);

        if (Status == NotificationTemplateStatus.Archived)
            return Result.Failure(NotificationTemplateErrors.AlreadyArchived);

        // Validate all enabled channels have content
        if (_channels.Contains(NotificationChannel.Email) && EmailContent == null)
            return Result.Failure(NotificationTemplateErrors.EmailChannelWithoutContent);

        if (_channels.Contains(NotificationChannel.Push) && PushContent == null)
            return Result.Failure(NotificationTemplateErrors.PushChannelWithoutContent);

        if (_channels.Contains(NotificationChannel.InApp) && InAppContent == null)
            return Result.Failure(NotificationTemplateErrors.InAppChannelWithoutContent);

        Status = NotificationTemplateStatus.Active;
        ActivatedBy = activatedBy;
        ActivatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationTemplateActivatedEvent(
            Id.Value,
            Name,
            activatedBy,
            ActivatedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Archives the template, removing it from active use.
    /// </summary>
    public Result Archive(Guid archivedBy)
    {
        if (Status == NotificationTemplateStatus.Archived)
            return Result.Failure(NotificationTemplateErrors.AlreadyArchived);

        Status = NotificationTemplateStatus.Archived;
        UpdatedBy = archivedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationTemplateArchivedEvent(
            Id.Value,
            Name,
            archivedBy,
            UpdatedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Updates template content for one or more channels.
    /// </summary>
    public Result UpdateContent(
        EmailTemplateContent? emailContent,
        PushTemplateContent? pushContent,
        InAppTemplateContent? inAppContent,
        Guid updatedBy)
    {
        // Validate content matches enabled channels
        if (emailContent != null && !_channels.Contains(NotificationChannel.Email))
            return Result.Failure(new Error(
                "NotificationTemplate.EmailChannelNotEnabled",
                "Email channel is not enabled for this template."));

        if (pushContent != null && !_channels.Contains(NotificationChannel.Push))
            return Result.Failure(new Error(
                "NotificationTemplate.PushChannelNotEnabled",
                "Push channel is not enabled for this template."));

        if (inAppContent != null && !_channels.Contains(NotificationChannel.InApp))
            return Result.Failure(new Error(
                "NotificationTemplate.InAppChannelNotEnabled",
                "InApp channel is not enabled for this template."));

        EmailContent = emailContent ?? EmailContent;
        PushContent = pushContent ?? PushContent;
        InAppContent = inAppContent ?? InAppContent;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationTemplateUpdatedEvent(
            Id.Value,
            Name,
            updatedBy,
            UpdatedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Adds a variable to the template's variable library.
    /// </summary>
    public Result AddVariable(TemplateVariable variable)
    {
        if (_variables.Count >= MaxVariables)
            return Result.Failure(NotificationTemplateErrors.TooManyVariables);

        if (_variables.Any(v => v.Name.Equals(variable.Name, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(NotificationTemplateErrors.DuplicateVariable);

        _variables.Add(variable);
        return Result.Success();
    }

    /// <summary>
    /// Removes a variable from the template.
    /// </summary>
    public Result RemoveVariable(string variableName)
    {
        var variable = _variables.FirstOrDefault(v =>
            v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));

        if (variable == null)
            return Result.Failure(NotificationTemplateErrors.VariableNotFound);

        _variables.Remove(variable);
        return Result.Success();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if the template is active and ready to use.
    /// </summary>
    public bool IsActive() => Status == NotificationTemplateStatus.Active;

    /// <summary>
    /// Checks if a specific channel is enabled for this template.
    /// </summary>
    public bool SupportsChannel(NotificationChannel channel) => _channels.Contains(channel);

    /// <summary>
    /// Checks if the template has all required content configured.
    /// </summary>
    public bool HasCompleteContent()
    {
        if (_channels.Contains(NotificationChannel.Email) && EmailContent == null)
            return false;

        if (_channels.Contains(NotificationChannel.Push) && PushContent == null)
            return false;

        if (_channels.Contains(NotificationChannel.InApp) && InAppContent == null)
            return false;

        return true;
    }

    #endregion
}
