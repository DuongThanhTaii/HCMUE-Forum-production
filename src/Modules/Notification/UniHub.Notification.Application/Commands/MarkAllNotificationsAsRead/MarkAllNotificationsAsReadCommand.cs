using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Commands.MarkAllNotificationsAsRead;

/// <summary>
/// Command to mark all notifications as read for a user.
/// </summary>
public sealed record MarkAllNotificationsAsReadCommand(Guid UserId) : ICommand<int>;
