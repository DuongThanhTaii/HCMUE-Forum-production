using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Commands.MarkNotificationAsRead;

/// <summary>
/// Command to mark a notification as read.
/// </summary>
public sealed record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : ICommand;
