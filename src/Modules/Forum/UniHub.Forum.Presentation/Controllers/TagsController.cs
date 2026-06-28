using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Forum.Application.Queries.GetPopularTags;
using UniHub.Forum.Application.Queries.GetTags;
using UniHub.Forum.Presentation.DTOs.Tags;

namespace UniHub.Forum.Presentation.Controllers;

[ApiController]
[Route("api/v1/tags")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly ISender _sender;

    public TagsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get a paginated list of tags with optional search and sorting
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TagListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTags(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool orderByUsage = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTagsQuery(pageNumber, pageSize, searchTerm, orderByUsage);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new TagListResponse
        {
            Tags = result.Value.Tags.Select(t => new TagResponse
            {
                Name = t.Name,
                PostCount = t.UsageCount
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages,
            HasPreviousPage = result.Value.HasPreviousPage,
            HasNextPage = result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    [HttpGet("popular")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PopularTagResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPopularTags(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPopularTagsQuery(count);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = result.Value.Select(t => new PopularTagResponse
        {
            Name = t.Name,
            PostCount = t.UsageCount,
            PopularityScore = t.UsageCount // Simple popularity calculation
        }).ToList();

        return Ok(ApiResponses.Success(response));
    }
}

public sealed record TagListResponse
{
    public IReadOnlyList<TagResponse> Tags { get; init; } = Array.Empty<TagResponse>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
