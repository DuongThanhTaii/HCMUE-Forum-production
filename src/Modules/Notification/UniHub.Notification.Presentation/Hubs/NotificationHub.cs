using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UniHub.Notification.Presentation.Hubs;

/// <summary>
/// SignalR client interface for receiving real-time notifications.
/// </summary>
public interface INotificationClient
{
    /// <summary>
    /// Receive a new notification in real-time.
    /// </summary>
    Task ReceiveNotification(NotificationMessage notification);

    /// <summary>
    /// Notification count updated (for badge display).
    /// </summary>
    Task UnreadCountUpdated(int count);
}

/// <summary>
/// Notification message sent to clients.
/// </summary>
public record NotificationMessage(
    Guid Id,
    string Title,
    string Message,
    string Type,
    DateTime CreatedAt,
    Dictionary<string, object>? Data = null);

/// <summary>
/// SignalR Hub for real-time notification delivery.
/// Clients connect to receive push notifications without polling.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            // Add user to their personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId.Value}");
            _logger.LogInformation("User {UserId} connected to NotificationHub", userId.Value);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId.Value}");
            _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId.Value);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client marks a notification as read.
    /// </summary>
    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        _logger.LogDebug("User {UserId} marked notification {NotificationId} as read",
            userId.Value, notificationId);

        // Could trigger MediatR command here if needed
    }

    /// <summary>
    /// Client marks all notifications as read.
    /// </summary>
    public async Task MarkAllAsRead()
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        _logger.LogDebug("User {UserId} marked all notifications as read", userId.Value);
        await Clients.Caller.UnreadCountUpdated(0);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
