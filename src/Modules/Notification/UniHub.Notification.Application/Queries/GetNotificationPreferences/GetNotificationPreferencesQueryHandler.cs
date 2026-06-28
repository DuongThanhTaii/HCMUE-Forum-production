using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.NotificationPreferences;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Queries.GetNotificationPreferences;

/// <summary>
/// Handler for getting notification preferences.
/// </summary>
public sealed class GetNotificationPreferencesQueryHandler : IQueryHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly ILogger<GetNotificationPreferencesQueryHandler> _logger;

    public GetNotificationPreferencesQueryHandler(
        INotificationPreferenceRepository preferenceRepository,
        ILogger<GetNotificationPreferencesQueryHandler> logger)
    {
        _preferenceRepository = preferenceRepository;
        _logger = logger;
    }

    public async Task<Result<NotificationPreferencesDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (preference is null)
        {
            // Return default preferences if none exist
            var dto = new NotificationPreferencesDto(
                request.UserId,
                EmailEnabled: true,
                PushEnabled: true,
                InAppEnabled: true,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow);

            return Result.Success(dto);
        }

        var preferencesDto = new NotificationPreferencesDto(
            preference.UserId,
            preference.EmailEnabled,
            preference.PushEnabled,
            preference.InAppEnabled,
            preference.CreatedAt,
            preference.UpdatedAt);

        return Result.Success(preferencesDto);
    }
}
