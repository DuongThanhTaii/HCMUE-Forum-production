using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.Notifications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Commands.MarkNotificationAsRead;

/// <summary>
/// Handler for marking a notification as read.
/// </summary>
public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ILogger<MarkNotificationAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
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
            return Result.Failure(new Error("Notification.Forbidden", "You don't have permission to access this notification"));
        }

        var result = notification.MarkAsRead();
        if (result.IsFailure)
        {
            return result;
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation(
            "Notification {NotificationId} marked as read by user {UserId}",
            request.NotificationId,
            request.UserId);

        return Result.Success();
    }
}
