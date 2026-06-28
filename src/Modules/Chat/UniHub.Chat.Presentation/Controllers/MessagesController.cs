using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;
using UniHub.Contracts;
using UniHub.Chat.Application.Commands.AddReaction;
using UniHub.Chat.Application.Commands.DeleteMessage;
using UniHub.Chat.Application.Commands.EditMessage;
using UniHub.Chat.Application.Commands.MarkMessageAsRead;
using UniHub.Chat.Application.Commands.RemoveReaction;
using UniHub.Chat.Application.Commands.ReportMessage;
using UniHub.Chat.Application.Commands.SendMessage;
using UniHub.Chat.Domain.Safety;
using UniHub.Chat.Application.Commands.SendMessageWithAttachments;
using UniHub.Chat.Application.Commands.UploadFile;
using UniHub.Chat.Application.Queries.GetMessageReadReceipts;
using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.Chat.Presentation.Hubs;

namespace UniHub.Chat.Presentation.Controllers;

/// <summary>
/// Controller for managing messages
/// </summary>
[ApiController]
[Route("api/v1/chat/messages")]
[Authorize]
[Produces("application/json")]
public class MessagesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IHubContext<ChatHub, IChatClient> _chatHub;

    public MessagesController(ISender sender, IHubContext<ChatHub, IChatClient> chatHub)
    {
        _sender = sender;
        _chatHub = chatHub;
    }

    private static string ConversationGroupName(Guid conversationId) => $"conversation:{conversationId}";

    /// <summary>
    /// Get messages for a conversation with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<MessageResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        [FromQuery] Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMessagesQuery(conversationId, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Conversation.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Edit an existing message (sender only).
    /// </summary>
    [HttpPatch("{messageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EditMessageResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditMessage(
        Guid messageId,
        [FromBody] EditMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(
            new EditMessageCommand(messageId, userId, request.Content),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code is "Message.NotSender" or "Message.CannotEditSystem" or "Message.AlreadyDeleted")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var v = result.Value;
        await _chatHub.Clients.Group(ConversationGroupName(v.ConversationId))
            .MessageEdited(new MessageEditedNotification(
                v.MessageId,
                v.ConversationId,
                v.Content,
                v.EditedAt));

        return Ok(ApiResponses.Success(v));
    }

    /// <summary>
    /// Soft-delete (recall) a message (sender only).
    /// </summary>
    [HttpDelete("{messageId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _sender.Send(
            new DeleteMessageCommand(messageId, userId),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code is "Message.NotSender" or "Message.CannotDeleteSystem" or "Message.AlreadyDeleted")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var v = result.Value;
        await _chatHub.Clients.Group(ConversationGroupName(v.ConversationId))
            .MessageDeleted(new MessageDeletedNotification(
                v.MessageId,
                v.ConversationId,
                v.DeletedAt));

        return Ok(ApiResponses.Success(v));
    }

    /// <summary>
    /// Send a text message to a conversation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SendMessageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var command = new SendMessageCommand(
            request.ConversationId,
            userId,
            request.Content,
            request.ReplyToMessageId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code.Contains("NotFound"))
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code.Contains("NotParticipant") || result.Error.Code == "Chat.UserBlocked")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new SendMessageResponse
        {
            MessageId = result.Value,
            SentAt = DateTime.UtcNow
        };

        var notification = new MessageNotification(
            result.Value,
            request.ConversationId,
            null,
            userId,
            GetUserName(),
            request.Content,
            "Text",
            response.SentAt,
            request.ReplyToMessageId);

        await _chatHub.Clients.Group(ConversationGroupName(request.ConversationId))
            .ReceiveMessage(notification);

        return CreatedAtAction(
            nameof(GetMessages),
            new { conversationId = request.ConversationId },
            ApiResponses.Success(response, "Message sent successfully"));
    }

    /// <summary>
    /// Upload a file for chat
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<UploadFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponses.Failure("No file provided"));
        }

        var userId = GetUserId();

        var contentType = file.ContentType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.FileName, out var inferred) ||
                string.IsNullOrWhiteSpace(inferred))
            {
                return BadRequest(
                    ApiResponses.Failure("Could not determine content type; use a file with a known extension."));
            }

            contentType = inferred;
        }

        using var stream = file.OpenReadStream();

        var command = new UploadFileCommand(
            file.FileName,
            stream,
            contentType,
            file.Length,
            userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new UploadFileResponse
        {
            FileName = result.Value.FileName,
            FileUrl = result.Value.FileUrl,
            FileSize = result.Value.FileSize,
            ContentType = result.Value.ContentType
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Send a message with file attachments
    /// </summary>
    [HttpPost("with-attachments")]
    [ProducesResponseType(typeof(ApiResponse<SendMessageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessageWithAttachments(
        [FromBody] SendMessageWithAttachmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var attachments = request.Attachments
            .Select(a => new AttachmentDto(
                a.FileName,
                a.FileUrl,
                a.FileSize,
                a.MimeType,
                a.ThumbnailUrl))
            .ToList();

        var command = new SendMessageWithAttachmentsCommand(
            request.ConversationId,
            userId,
            request.Content,
            attachments,
            request.ReplyToMessageId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code.Contains("NotFound"))
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code.Contains("NotParticipant") || result.Error.Code == "Chat.UserBlocked")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new SendMessageResponse
        {
            MessageId = result.Value,
            SentAt = DateTime.UtcNow
        };

        var notification = new MessageNotification(
            result.Value,
            request.ConversationId,
            null,
            userId,
            GetUserName(),
            request.Content ?? "Sent an attachment",
            "File",
            response.SentAt,
            request.ReplyToMessageId);

        await _chatHub.Clients.Group(ConversationGroupName(request.ConversationId))
            .ReceiveMessage(notification);

        return CreatedAtAction(
            nameof(GetMessages),
            new { conversationId = request.ConversationId },
            ApiResponses.Success(response, "Message sent successfully"));
    }

    /// <summary>
    /// Add an emoji reaction to a message
    /// </summary>
    [HttpPost("{messageId}/reactions")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReaction(
        Guid messageId,
        [FromBody] AddReactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new AddReactionCommand(messageId, userId, request.Emoji);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Reaction added successfully"));
    }

    /// <summary>
    /// Remove an emoji reaction from a message
    /// </summary>
    [HttpDelete("{messageId}/reactions/{emoji}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveReaction(
        Guid messageId,
        string emoji,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new RemoveReactionCommand(messageId, userId, emoji);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Reaction removed successfully"));
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    [HttpPost("{messageId}/read")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkMessageAsRead(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new MarkMessageAsReadCommand(messageId, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Message marked as read"));
    }

    [HttpPost("{messageId:guid}/report")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportMessage(
        Guid messageId,
        [FromBody] ReportMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var reason = Enum.TryParse<ChatMessageReportReason>(request.Reason, true, out var parsed)
            ? parsed
            : ChatMessageReportReason.Other;

        var result = await _sender.Send(
            new ReportMessageCommand(userId, messageId, reason, request.Description),
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            if (result.Error.Code is "Conversation.NotParticipant")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponses.Success("Report submitted"));
    }

    /// <summary>
    /// Get read receipts for a message
    /// </summary>
    [HttpGet("{messageId}/read-receipts")]
    [ProducesResponseType(typeof(ApiResponse<List<ReadReceiptResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReadReceipts(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMessageReadReceiptsQuery(messageId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Message.NotFound")
            {
                return NotFound(ApiResponses.Failure(result.Error.Message));
            }

            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private string GetUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst("name")?.Value 
            ?? "Unknown User";
    }
}

/// <summary>
/// Body for editing a message
/// </summary>
public record EditMessageRequest(string Content);

/// <summary>
/// Request to send a message
/// </summary>
public record SendMessageRequest(
    Guid ConversationId,
    string Content,
    Guid? ReplyToMessageId = null);

/// <summary>
/// Request to send a message with attachments
/// </summary>
public record SendMessageWithAttachmentsRequest(
    Guid ConversationId,
    string? Content,
    List<AttachmentRequest> Attachments,
    Guid? ReplyToMessageId = null);

/// <summary>
/// Attachment data for request
/// </summary>
public record AttachmentRequest(
    string FileName,
    string FileUrl,
    long FileSize,
    string MimeType,
    string? ThumbnailUrl = null);

/// <summary>
/// Response after sending a message
/// </summary>
public record SendMessageResponse
{
    public Guid MessageId { get; init; }
    public DateTime SentAt { get; init; }
}

/// <summary>
/// Response after uploading a file
/// </summary>
public record UploadFileResponse
{
    public string FileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = string.Empty;
}

/// <summary>
/// Request to add a reaction to a message
/// </summary>
public record AddReactionRequest(string Emoji);

public record ReportMessageRequest(string Reason, string? Description);
