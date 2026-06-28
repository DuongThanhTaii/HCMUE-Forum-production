using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.Notifications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Queries.GetNotifications;

/// <summary>
/// Handler for getting paginated notifications.
/// </summary>
public sealed class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetNotificationsQueryHandler> _logger;

    public GetNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ILogger<GetNotificationsQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result<GetNotificationsResponse>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            return Result.Failure<GetNotificationsResponse>(
                new Error("GetNotifications.InvalidPageNumber", "Page number must be greater than 0"));
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetNotificationsResponse>(
                new Error("GetNotifications.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        var (notifications, totalCount) = await _notificationRepository.GetByRecipientAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var notificationDtos = notifications.Select(n => new NotificationDto(
            n.Id.Value,
            n.Content.Subject,
            n.Content.Body,
            n.Content.ActionUrl,
            n.Content.IconUrl,
            n.Status.ToString(),
            n.Channel.ToString(),
            n.CreatedAt,
            n.ReadAt,
            n.IsRead()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new GetNotificationsResponse(
            notificationDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}
