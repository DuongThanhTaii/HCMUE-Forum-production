namespace UniHub.Notification.Infrastructure.Services.Notifications;

/// <summary>
/// Configuration settings for Web Push notifications.
/// VAPID (Voluntary Application Server Identification) keys are required for push notifications.
/// Generate keys at: https://vapidkeys.com/ or use: npx web-push generate-vapid-keys
/// </summary>
public sealed class WebPushSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "WebPush";

    /// <summary>
    /// VAPID subject (mailto: or https:// URL identifying your application).
    /// Example: "mailto:admin@unihub.com" or "https://unihub.com"
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// VAPID public key (used by client-side to subscribe).
    /// </summary>
    public string PublicKey { get; init; } = string.Empty;

    /// <summary>
    /// VAPID private key (kept secret on server).
    /// </summary>
    public string PrivateKey { get; init; } = string.Empty;

    /// <summary>
    /// Maximum number of retry attempts for failed push notifications.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Timeout for push notification requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
}
