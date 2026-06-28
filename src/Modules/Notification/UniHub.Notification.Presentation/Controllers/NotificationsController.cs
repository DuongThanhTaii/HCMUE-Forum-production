using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniHub.Contracts;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.Commands.DeleteNotification;
using UniHub.Notification.Application.Commands.MarkAllNotificationsAsRead;
using UniHub.Notification.Application.Commands.MarkNotificationAsRead;
using UniHub.Notification.Application.Commands.UpdateNotificationPreferences;
using UniHub.Notification.Application.Contracts;
using UniHub.Notification.Application.Queries.GetNotificationPreferences;
using UniHub.Notification.Application.Queries.GetNotifications;
using UniHub.Notification.Application.Queries.GetUnreadCount;
using UniHub.Notification.Domain.Notifications;
using UniHub.SharedKernel.Persistence;

namespace UniHub.Notification.Presentation.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsController(
        ISender sender,
        IPermissionChecker permissionChecker,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        INotificationPusher notificationPusher,
        IEmailNotificationService emailNotificationService,
        IUnitOfWork unitOfWork)
    {
        _sender = sender;
        _permissionChecker = permissionChecker;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _notificationPusher = notificationPusher;
        _emailNotificationService = emailNotificationService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("broadcast/home")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BroadcastHomeAnnouncement(
        [FromBody] BroadcastHomeAnnouncementRequest request,
        CancellationToken cancellationToken = default)
    {
        var actorId = GetCurrentUserId();
        if (actorId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var hasPermission = await _permissionChecker.HasAnyPermissionAsync(
            UserId.Create(actorId),
            new[] { "admin.system.manage", "forum.reports.review" },
            cancellationToken);
        if (!hasPermission)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure("Only Admin/Moderator can broadcast announcements."));
        }

        var contentResult = NotificationContent.Create(
            request.Title.Trim(),
            request.Message.Trim(),
            "/home?announcement=true");
        if (contentResult.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(contentResult.Error.Message));
        }

        var users = await _userRepository.GetAllAsync(cancellationToken);
        var sentInApp = 0;
        var sentEmail = 0;

        foreach (var user in users)
        {
            var inAppResult = global::UniHub.Notification.Domain.Notifications.Notification.Create(
                user.Id.Value,
                global::UniHub.Notification.Domain.NotificationTemplates.NotificationChannel.InApp,
                contentResult.Value);
            if (inAppResult.IsFailure)
            {
                continue;
            }

            var inAppNotification = inAppResult.Value;
            inAppNotification.MarkAsSent();
            await _notificationRepository.AddAsync(inAppNotification, cancellationToken);
            sentInApp++;

            await _notificationPusher.PushAsync(
                user.Id.Value,
                inAppNotification.Id.Value,
                inAppNotification.Content.Subject,
                inAppNotification.Content.Body,
                "announcement",
                inAppNotification.CreatedAt,
                inAppNotification.Content.ActionUrl,
                cancellationToken);

            if (!request.SendEmail)
            {
                continue;
            }

            var emailResult = global::UniHub.Notification.Domain.Notifications.Notification.Create(
                user.Id.Value,
                global::UniHub.Notification.Domain.NotificationTemplates.NotificationChannel.Email,
                contentResult.Value);
            if (emailResult.IsFailure)
            {
                continue;
            }

            var emailNotification = emailResult.Value;
            var sendResult = await _emailNotificationService.SendAsync(emailNotification, cancellationToken);
            if (sendResult.IsSuccess)
            {
                emailNotification.MarkAsSent();
                sentEmail++;
            }
            else
            {
                emailNotification.MarkAsFailed(sendResult.Error.Message);
            }
            await _notificationRepository.AddAsync(emailNotification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponses.Success(
            (object)new { sentInApp, sentEmail },
            "Home announcement broadcasted successfully"));
    }

    /// <summary>
    /// Get paginated notifications for the current user
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetNotificationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var query = new GetNotificationsQuery(userId, pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get unread notification count for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of unread notifications</returns>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var query = new GetUnreadCountQuery(userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success((object)new { count = result.Value }));
    }

    /// <summary>
    /// Mark a notification as read
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPost("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var command = new MarkNotificationAsReadCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Notification.NotFound" => NotFound(ApiResponses.Failure(result.Error.Message)),
                "Notification.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message)),
                _ => BadRequest(ApiResponses.Failure(result.Error.Message))
            };
        }

        return Ok(ApiResponses.Success("Notification marked as read"));
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of notifications marked as read</returns>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var command = new MarkAllNotificationsAsReadCommand(userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success((object)new { count = result.Value }, "Notifications marked as read"));
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var command = new DeleteNotificationCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Notification.NotFound" => NotFound(ApiResponses.Failure(result.Error.Message)),
                "Notification.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message)),
                _ => BadRequest(ApiResponses.Failure(result.Error.Message))
            };
        }

        return Ok(ApiResponses.Success("Notification deleted successfully"));
    }

    /// <summary>
    /// Get notification preferences for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification preferences</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(ApiResponse<NotificationPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var query = new GetNotificationPreferencesQuery(userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Update notification preferences for the current user
    /// </summary>
    /// <param name="request">Notification preferences update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(ApiResponses.Failure("Invalid user token"));
        }

        var command = new UpdateNotificationPreferencesCommand(
            userId,
            request.EmailEnabled,
            request.PushEnabled,
            request.InAppEnabled);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Notification preferences updated successfully"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public sealed record BroadcastHomeAnnouncementRequest(
    string Title,
    string Message,
    bool SendEmail);
