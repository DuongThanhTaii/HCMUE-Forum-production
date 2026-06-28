namespace UniHub.Notification.Application.Queries.GetNotifications;

/// <summary>
/// Response for getting notifications query.
/// </summary>
public sealed record GetNotificationsResponse(
    List<NotificationDto> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// DTO for a notification.
/// </summary>
public sealed record NotificationDto(
    Guid Id,
    string Subject,
    string Body,
    string? ActionUrl,
    string? IconUrl,
    string Status,
    string Channel,
    DateTime CreatedAt,
    DateTime? ReadAt,
    bool IsRead);
