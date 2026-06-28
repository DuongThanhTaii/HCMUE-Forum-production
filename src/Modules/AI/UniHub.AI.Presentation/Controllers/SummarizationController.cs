using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;

namespace UniHub.AI.Presentation.Controllers;

/// <summary>
/// Controller for document summarization operations.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("ai")]
public class SummarizationController : ControllerBase
{
    private readonly IDocumentSummarizationService _summarizationService;

    public SummarizationController(IDocumentSummarizationService summarizationService)
    {
        _summarizationService = summarizationService ?? throw new ArgumentNullException(nameof(summarizationService));
    }

    /// <summary>
    /// Summarize a document or text content.
    /// </summary>
    /// <param name="request">Summarization request with content and options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SummarizationResponse with summary and key points.</returns>
    [HttpPost("summarize")]
    [ProducesResponseType(typeof(ApiResponse<SummarizationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Summarize(
        [FromBody] SummarizationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponses.Failure("Content is required for summarization."));
        }

        try
        {
            var response = await _summarizationService.SummarizeAsync(request, cancellationToken);
            return Ok(ApiResponses.Success(response));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponses.Failure(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponses.Failure(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while summarizing content. {ex.Message}"));
        }
    }

    /// <summary>
    /// Extract key points from text without full summarization.
    /// </summary>
    /// <param name="request">Request containing content and max points.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of key points.</returns>
    [HttpPost("summarize/keypoints")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExtractKeyPoints(
        [FromBody] KeyPointsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponses.Failure("Content is required."));
        }

        try
        {
            var keyPoints = await _summarizationService.ExtractKeyPointsAsync(
                request.Content, 
                request.MaxPoints,
                cancellationToken);
            
            return Ok(ApiResponses.Success((object)new { keyPoints }));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while extracting key points. {ex.Message}"));
        }
    }

    /// <summary>
    /// Detect the language of text content.
    /// </summary>
    /// <param name="content">Text content to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detected language code.</returns>
    [HttpGet("summarize/detect-language")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DetectLanguage(
        [FromQuery] string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(ApiResponses.Failure("Content parameter is required."));
        }

        try
        {
            var language = await _summarizationService.DetectLanguageAsync(content, cancellationToken);
            return Ok(ApiResponses.Success((object)new { language }));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while detecting language. {ex.Message}"));
        }
    }

    /// <summary>
    /// Clear summarization cache.
    /// </summary>
    /// <param name="cacheKey">Optional specific cache key to clear. If omitted, clears all cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    [HttpDelete("summarize/cache")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearCache(
        [FromQuery] string? cacheKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                await _summarizationService.ClearAllCacheAsync();
            }
            else
            {
                await _summarizationService.ClearCacheAsync(cacheKey);
            }

            return Ok(ApiResponses.Success("Cache cleared successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while clearing cache. {ex.Message}"));
        }
    }
}

/// <summary>
/// Request model for extracting key points.
/// </summary>
public class KeyPointsRequest
{
    /// <summary>
    /// Content to extract key points from.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of key points to extract.
    /// </summary>
    public int MaxPoints { get; set; } = 5;
}
