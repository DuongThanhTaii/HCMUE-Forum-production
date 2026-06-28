using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.Notifications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Commands.DeleteNotification;

/// <summary>
/// Handler for deleting a notification.
/// </summary>
public sealed class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;

    public DeleteNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ILogger<DeleteNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notificationId = NotificationId.Create(request.NotificationId);
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        // Verify the notification belongs to the user
        if (notification.RecipientId != request.UserId)
        {
            return Result.Failure(new Error("Notification.Forbidden", "You don't have permission to delete this notification"));
        }

        await _notificationRepository.DeleteAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Notification {NotificationId} deleted by user {UserId}",
            request.NotificationId,
            request.UserId);

        return Result.Success();
    }
}
