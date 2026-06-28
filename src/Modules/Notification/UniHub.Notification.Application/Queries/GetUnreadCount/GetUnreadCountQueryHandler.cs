using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Queries.GetUnreadCount;

/// <summary>
/// Handler for getting unread notification count.
/// </summary>
public sealed class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;

    public GetUnreadCountQueryHandler(
        INotificationRepository notificationRepository,
        ILogger<GetUnreadCountQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(request.UserId, cancellationToken);
        return Result.Success(count);
    }
}
