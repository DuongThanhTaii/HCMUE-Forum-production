using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.NotificationPreferences;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Commands.UpdateNotificationPreferences;

/// <summary>
/// Handler for updating notification preferences.
/// </summary>
public sealed class UpdateNotificationPreferencesCommandHandler : ICommandHandler<UpdateNotificationPreferencesCommand>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly ILogger<UpdateNotificationPreferencesCommandHandler> _logger;

    public UpdateNotificationPreferencesCommandHandler(
        INotificationPreferenceRepository preferenceRepository,
        ILogger<UpdateNotificationPreferencesCommandHandler> logger)
    {
        _preferenceRepository = preferenceRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (preference is null)
        {
            // Create new preferences if they don't exist
            var createResult = NotificationPreference.Create(request.UserId);
            if (createResult.IsFailure)
            {
                return createResult;
            }

            preference = createResult.Value;
            preference.UpdateAll(request.EmailEnabled, request.PushEnabled, request.InAppEnabled);
            await _preferenceRepository.AddAsync(preference, cancellationToken);
        }
        else
        {
            // Update existing preferences
            preference.UpdateAll(request.EmailEnabled, request.PushEnabled, request.InAppEnabled);
            await _preferenceRepository.UpdateAsync(preference, cancellationToken);
        }

        _logger.LogInformation(
            "Updated notification preferences for user {UserId}: Email={EmailEnabled}, Push={PushEnabled}, InApp={InAppEnabled}",
            request.UserId,
            request.EmailEnabled,
            request.PushEnabled,
            request.InAppEnabled);

        return Result.Success();
    }
}
