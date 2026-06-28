using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniHub.Contracts;
using UniHub.Chat.Application.Commands.AddModerator;
using UniHub.Chat.Application.Commands.CreateChannel;
using UniHub.Chat.Application.Commands.JoinChannel;
using UniHub.Chat.Application.Commands.LeaveChannel;
using UniHub.Chat.Application.Commands.RemoveModerator;
using UniHub.Chat.Application.Commands.UpdateChannel;
using UniHub.Chat.Application.Queries.GetChannels;

namespace UniHub.Chat.Presentation.Controllers;

/// <summary>
/// Controller for managing channels
/// </summary>
[ApiController]
[Route("api/v1/chat/channels")]
[Authorize]
[Produces("application/json")]
public class ChannelsController : ControllerBase
{
    private readonly ISender _sender;

    public ChannelsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all public channels (for discovery)
    /// </summary>
    [HttpGet("public")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChannelResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPublicChannels(CancellationToken cancellationToken = default)
    {
        var query = new GetChannelsQuery(PublicOnly: true);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get channels where the current user is a member
    /// </summary>
    [HttpGet("my-channels")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChannelResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyChannels(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetChannelsQuery(UserId: userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Create a new channel
    /// </summary>
    /// <param name="request">Request containing channel information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateChannelResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateChannel(
        [FromBody] CreateChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new CreateChannelCommand(
            request.Name,
            request.Description,
            request.IsPublic,
            userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new CreateChannelResponse
        {
            ChannelId = result.Value
        };

        return CreatedAtAction(
            nameof(GetMyChannels),
            new { id = result.Value },
            ApiResponses.Success(response, "Channel created successfully"));
    }

    /// <summary>
    /// Join a channel
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id:guid}/join")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> JoinChannel(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new JoinChannelCommand(id, userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Channel.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Successfully joined channel"));
    }

    /// <summary>
    /// Leave a channel
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id:guid}/leave")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveChannel(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new LeaveChannelCommand(id, userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Channel.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Successfully left channel"));
    }

    /// <summary>
    /// Add a moderator to a channel (owner only)
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="request">Request containing the user ID to promote</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id:guid}/moderators")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddModerator(
        Guid id,
        [FromBody] ModeratorRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new AddModeratorCommand(id, request.UserId, userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Channel.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code.Contains("NotAuthorized") || result.Error.Code.Contains("NotOwner"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Moderator added successfully"));
    }

    /// <summary>
    /// Remove a moderator from a channel (owner only)
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="moderatorId">Moderator ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete("{id:guid}/moderators/{moderatorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveModerator(
        Guid id,
        Guid moderatorId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new RemoveModeratorCommand(id, moderatorId, userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Channel.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code.Contains("NotAuthorized") || result.Error.Code.Contains("NotOwner"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Moderator removed successfully"));
    }

    /// <summary>
    /// Update channel information (moderators and owner only)
    /// </summary>
    /// <param name="id">Channel ID</param>
    /// <param name="request">Request containing updated information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChannel(
        Guid id,
        [FromBody] UpdateChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new UpdateChannelCommand(
            id,
            request.Name,
            request.Description,
            userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Channel.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code.Contains("NotAuthorized") || result.Error.Code.Contains("NotModerator"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Channel updated successfully"));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}

/// <summary>
/// Request to create a channel
/// </summary>
public record CreateChannelRequest(string Name, string? Description, bool IsPublic);

/// <summary>
/// Response after creating a channel
/// </summary>
public record CreateChannelResponse
{
    public Guid ChannelId { get; init; }
}

/// <summary>
/// Request to add or remove a moderator
/// </summary>
public record ModeratorRequest(Guid UserId);

/// <summary>
/// Request to update channel information
/// </summary>
public record UpdateChannelRequest(string? Name, string? Description);
