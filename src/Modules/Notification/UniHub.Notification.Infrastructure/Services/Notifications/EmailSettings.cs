namespace UniHub.Notification.Infrastructure.Services.Notifications;

/// <summary>
/// Configuration settings for Email notifications using SMTP.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// SMTP server host (e.g., smtp.gmail.com, smtp.sendgrid.net).
    /// </summary>
    public string SmtpHost { get; init; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL, 25 for non-SSL).
    /// </summary>
    public int SmtpPort { get; init; } = 587;

    /// <summary>
    /// SMTP username/email.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// SMTP password or API key.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Default sender email address.
    /// </summary>
    public string FromEmail { get; init; } = string.Empty;

    /// <summary>
    /// Default sender display name.
    /// </summary>
    public string FromName { get; init; } = string.Empty;

    /// <summary>
    /// Enable SSL/TLS encryption.
    /// </summary>
    public bool EnableSsl { get; init; } = true;

    /// <summary>
    /// Connection timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for failed emails.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;
}
