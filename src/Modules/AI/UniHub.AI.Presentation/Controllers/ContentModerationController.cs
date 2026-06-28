using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;

namespace UniHub.AI.Presentation.Controllers;

/// <summary>
/// Controller for content moderation operations.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("ai")]
public class ContentModerationController : ControllerBase
{
    private readonly IContentModerationService _moderationService;

    public ContentModerationController(IContentModerationService moderationService)
    {
        _moderationService = moderationService ?? throw new ArgumentNullException(nameof(moderationService));
    }

    /// <summary>
    /// Moderate content for toxicity, spam, and inappropriate material.
    /// </summary>
    /// <param name="request">Moderation request with content to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ModerationResponse with violation details and action recommendation.</returns>
    [HttpPost("moderate")]
    [ProducesResponseType(typeof(ApiResponse<ModerationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ModerateContent(
        [FromBody] ModerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponses.Failure("Content is required for moderation."));
        }

        try
        {
            var response = await _moderationService.ModerateAsync(request, cancellationToken);
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
                ApiResponses.Failure($"An error occurred while moderating content. {ex.Message}"));
        }
    }

    /// <summary>
    /// Check if content is safe without detailed analysis.
    /// </summary>
    /// <param name="content">Content to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Simple boolean indicating if content is safe.</returns>
    [HttpGet("moderate/check")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckContentSafety(
        [FromQuery] string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(ApiResponses.Failure("Content parameter is required."));
        }

        try
        {
            var request = new ModerationRequest
            {
                Content = content,
                ContentType = ContentType.Text
            };

            var response = await _moderationService.ModerateAsync(request, cancellationToken);
            
            return Ok(ApiResponses.Success((object)new
            {
                isSafe = response.IsSafe,
                riskScore = response.ConfidenceScore,
                isBlocked = response.IsBlocked,
                requiresReview = response.RequiresReview
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while checking content safety. {ex.Message}"));
        }
    }
}
