using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Queries.GetUnreadCount;

/// <summary>
/// Query to get unread notification count for a user.
/// </summary>
public sealed record GetUnreadCountQuery(Guid UserId) : IQuery<int>;
