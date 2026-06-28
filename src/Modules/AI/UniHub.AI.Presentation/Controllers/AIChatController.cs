using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Presentation.Controllers;

/// <summary>
/// Controller for AI chatbot (UniBot) operations.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("ai")]
public class AIChatController : ControllerBase
{
    private readonly IUniBotService _uniBotService;
    private readonly IConversationService _conversationService;

    public AIChatController(
        IUniBotService uniBotService,
        IConversationService conversationService)
    {
        _uniBotService = uniBotService ?? throw new ArgumentNullException(nameof(uniBotService));
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
    }

    /// <summary>
    /// Send a message to UniBot chatbot.
    /// </summary>
    /// <param name="request">Chat request with message and optional conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ChatResponse with bot's reply.</returns>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ApiResponse<ChatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(ApiResponses.Failure("Message is required."));
        }

        try
        {
            var response = await _uniBotService.ChatAsync(request, cancellationToken);
            return Ok(ApiResponses.Success(response));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponses.Failure(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while processing your request. {ex.Message}"));
        }
    }

    /// <summary>
    /// Get all conversations for a user.
    /// </summary>
    /// <param name="userId">User ID to filter conversations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of conversations.</returns>
    [HttpGet("conversations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Conversation>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConversations(
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(ApiResponses.Failure("userId parameter is required."));
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest(ApiResponses.Failure("Invalid userId format."));
        }

        try
        {
            var conversations = await _conversationService.GetByUserIdAsync(userGuid, cancellationToken: cancellationToken);
            return Ok(ApiResponses.Success(conversations));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while retrieving conversations. {ex.Message}"));
        }
    }

    /// <summary>
    /// Get a specific conversation by ID.
    /// </summary>
    /// <param name="id">Conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Conversation details.</returns>
    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<Conversation>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConversation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetByIdAsync(id, cancellationToken);
        
        if (conversation == null)
        {
            return NotFound(ApiResponses.Failure($"Conversation with ID '{id}' not found."));
        }

        return Ok(ApiResponses.Success(conversation));
    }

    /// <summary>
    /// Close/delete a conversation.
    /// </summary>
    /// <param name="id">Conversation ID to close.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    [HttpDelete("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var closed = await _conversationService.CloseConversationAsync(id, cancellationToken);
        
        if (!closed)
        {
            return NotFound(ApiResponses.Failure($"Conversation with ID '{id}' not found."));
        }

        return Ok(ApiResponses.Success("Conversation deleted successfully"));
    }
}
