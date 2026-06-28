using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;

namespace UniHub.AI.Presentation.Controllers;

/// <summary>
/// Controller for smart search operations with AI-powered query understanding.
/// </summary>
[ApiController]
[Route("api/v1/ai")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("ai")]
public class SmartSearchController : ControllerBase
{
    private readonly ISmartSearchService _searchService;

    public SmartSearchController(ISmartSearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    /// <summary>
    /// Perform smart search with AI-powered query understanding.
    /// </summary>
    /// <param name="q">Search query.</param>
    /// <param name="type">Type of content to search (All, Posts, Questions, etc.).</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Results per page (default: 10).</param>
    /// <param name="suggestions">Include search suggestions (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SearchResponse with results, pagination, and suggestions.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] SearchType type = SearchType.All,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool suggestions = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponses.Failure("Search query 'q' parameter is required."));
        }

        try
        {
            var request = new SearchRequest
            {
                Query = q,
                SearchType = type,
                Category = category,
                Page = page,
                PageSize = pageSize,
                IncludeSuggestions = suggestions
            };

            var response = await _searchService.SearchAsync(request, cancellationToken);
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
                ApiResponses.Failure($"An error occurred while performing search. {ex.Message}"));
        }
    }

    /// <summary>
    /// Get search suggestions based on partial query.
    /// </summary>
    /// <param name="q">Partial query text.</param>
    /// <param name="limit">Maximum number of suggestions (default: 5).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of suggested queries.</returns>
    [HttpGet("search/suggestions")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponses.Failure("Query 'q' parameter is required."));
        }

        try
        {
            var suggestions = await _searchService.GetSuggestionsAsync(q, limit, cancellationToken);
            return Ok(ApiResponses.Success((object)new { query = q, suggestions }));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while getting suggestions. {ex.Message}"));
        }
    }

    /// <summary>
    /// Understand query intent and get expanded query with AI.
    /// </summary>
    /// <param name="q">Query to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>QueryUnderstanding with intent, expanded query, and entities.</returns>
    [HttpGet("search/understand")]
    [ProducesResponseType(typeof(ApiResponse<QueryUnderstanding>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnderstandQuery(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponses.Failure("Query 'q' parameter is required."));
        }

        try
        {
            var understanding = await _searchService.UnderstandQueryAsync(q, cancellationToken);
            return Ok(ApiResponses.Success(understanding));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponses.Failure($"An error occurred while understanding query. {ex.Message}"));
        }
    }

    /// <summary>
    /// Advanced search with full request body.
    /// </summary>
    /// <param name="request">Complete search request with all filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SearchResponse with results and metadata.</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdvancedSearch(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(ApiResponses.Failure("Search query is required."));
        }

        try
        {
            var response = await _searchService.SearchAsync(request, cancellationToken);
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
                ApiResponses.Failure($"An error occurred while performing search. {ex.Message}"));
        }
    }
}
