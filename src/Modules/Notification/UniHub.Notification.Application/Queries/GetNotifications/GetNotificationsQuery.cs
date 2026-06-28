using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Queries.GetNotifications;

/// <summary>
/// Query to get paginated notifications for a user.
/// </summary>
public sealed record GetNotificationsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<GetNotificationsResponse>;
