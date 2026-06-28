using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Results;
using WebPush;

namespace UniHub.Notification.Infrastructure.Services.Notifications;

/// <summary>
/// Web Push notification service implementation using the WebPush library.
/// Sends push notifications to web browsers supporting the Web Push API.
/// </summary>
public sealed class WebPushNotificationService : IPushNotificationService
{
    private readonly WebPushClient _webPushClient;
    private readonly WebPushSettings _settings;
    private readonly ILogger<WebPushNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPushNotificationService"/> class.
    /// </summary>
    /// <param name="settings">Web Push configuration settings.</param>
    /// <param name="logger">Logger instance.</param>
    public WebPushNotificationService(
        IOptions<WebPushSettings> settings,
        ILogger<WebPushNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _webPushClient = new WebPush.WebPushClient();
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Push;

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        Domain.Notifications.Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending push notification {NotificationId} to recipient {RecipientId}",
                notification.Id.Value,
                notification.RecipientId);

            // In a real implementation, you would need to retrieve the user's push subscription
            // from a repository/database. For now, this is a placeholder.
            
            // TODO: Implement IPushSubscriptionRepository to retrieve user's push subscription
            // var subscription = await _pushSubscriptionRepository.GetByUserIdAsync(
            //     notification.RecipientId, 
            //     cancellationToken);
            
            // if (subscription is null)
            // {
            //     return Result.Failure(
            //         new Error(
            //             "PushSubscription.NotFound",
            //             $"No push subscription found for user {notification.RecipientId}"));
            // }

            // For now, return failure indicating subscription lookup is not implemented
            return Result.Failure(
                new Error(
                    "WebPush.NotImplemented",
                    "Push subscription repository not yet implemented. Will be added in future iteration."));

            // When subscription is available, uncomment this:
            // return await SendPushAsync(
            //     subscription.Endpoint,
            //     subscription.P256dh,
            //     subscription.Auth,
            //     notification.Content.Subject,
            //     notification.Content.Body,
            //     notification.Content.IconUrl,
            //     notification.Content.ActionUrl,
            //     cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send push notification {NotificationId}",
                notification.Id.Value);

            return Result.Failure(
                new Error(
                    "WebPush.SendFailed",
                    $"Failed to send push notification: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> SendPushAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string body,
        string? icon = null,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return Result.Failure(
                    new Error("WebPush.EndpointRequired", "Push endpoint is required"));
            }

            if (string.IsNullOrWhiteSpace(p256dh))
            {
                return Result.Failure(
                    new Error("WebPush.P256dhRequired", "P256DH key is required"));
            }

            if (string.IsNullOrWhiteSpace(auth))
            {
                return Result.Failure(
                    new Error("WebPush.AuthRequired", "Auth key is required"));
            }

            // Create push subscription
            var pushSubscription = new PushSubscription(endpoint, p256dh, auth);

            // Create notification payload
            var payload = new
            {
                title,
                body,
                icon,
                data = new
                {
                    url
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload);

            // Create VAPID details
            var vapidDetails = new VapidDetails(
                _settings.Subject,
                _settings.PublicKey,
                _settings.PrivateKey);

            _logger.LogInformation(
                "Sending web push notification to endpoint: {Endpoint}",
                endpoint);

            // Send push notification with retry logic
            for (int attempt = 1; attempt <= _settings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    await _webPushClient.SendNotificationAsync(
                        pushSubscription,
                        payloadJson,
                        vapidDetails);

                    _logger.LogInformation(
                        "Successfully sent push notification on attempt {Attempt}",
                        attempt);

                    return Result.Success();
                }
                catch (WebPushException webPushEx) when (attempt < _settings.MaxRetryAttempts)
                {
                    _logger.LogWarning(
                        webPushEx,
                        "Push notification attempt {Attempt} failed with status {StatusCode}. Retrying...",
                        attempt,
                        webPushEx.StatusCode);

                    // If subscription is expired/invalid, don't retry
                    if (webPushEx.StatusCode == System.Net.HttpStatusCode.Gone ||
                        webPushEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogError(
                            "Push subscription is invalid or expired. Status: {StatusCode}",
                            webPushEx.StatusCode);

                        return Result.Failure(
                            new Error(
                                "WebPush.SubscriptionInvalid",
                                "Push subscription is no longer valid or has expired"));
                    }

                    // Wait before retry (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }

            // All retry attempts exhausted
            _logger.LogError(
                "Failed to send push notification after {MaxAttempts} attempts",
                _settings.MaxRetryAttempts);

            return Result.Failure(
                new Error(
                    "WebPush.MaxRetriesExceeded",
                    $"Failed to send push notification after {_settings.MaxRetryAttempts} attempts"));
        }
        catch (WebPushException webPushEx)
        {
            _logger.LogError(
                webPushEx,
                "Web push error: {Message}, Status: {StatusCode}",
                webPushEx.Message,
                webPushEx.StatusCode);

            return Result.Failure(
                new Error(
                    "WebPush.SendFailed",
                    $"Failed to send push notification: {webPushEx.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error sending push notification: {Message}",
                ex.Message);

            return Result.Failure(
                new Error(
                    "WebPush.UnexpectedError",
                    $"Unexpected error: {ex.Message}"));
        }
    }
}
