using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Career.Application.Commands.JobPostings.CloseJobPosting;
using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.Career.Application.Commands.JobPostings.PublishJobPosting;
using UniHub.Career.Application.Commands.JobPostings.UpdateJobPosting;
using UniHub.Career.Application.Commands.SavedJobs.SaveJob;
using UniHub.Career.Application.Commands.SavedJobs.UnsaveJob;
using UniHub.Career.Application.Queries.JobPostings.GetJobPostingById;
using UniHub.Career.Application.Queries.JobPostings.GetJobPostings;
using UniHub.Career.Application.Queries.JobPostings.SearchJobPostings;
using UniHub.Career.Application.Queries.SavedJobs.GetSavedJobs;
using UniHub.Career.Application.Queries.SavedJobs.IsSaved;
using UniHub.Contracts;

namespace UniHub.Career.Presentation.Controllers;

[Route("api/v1/jobs")]
[Produces("application/json")]
[Authorize]
public class JobPostingsController : BaseApiController
{
    private readonly ISender _sender;

    public JobPostingsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all job postings with filters
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<JobPostingListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? companyId,
        [FromQuery] string? jobType,
        [FromQuery] string? experienceLevel,
        [FromQuery] string? status,
        [FromQuery] string? city,
        [FromQuery] bool? isRemote,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetJobPostingsQuery(
            Page: page,
            PageSize: pageSize,
            CompanyId: companyId,
            JobType: null, // TODO: Parse from string
            ExperienceLevel: null, // TODO: Parse from string
            Status: null, // TODO: Parse from string
            City: city,
            IsRemote: isRemote,
            SearchTerm: searchTerm);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Search job postings with advanced filters
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<JobPostingSearchResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? keywords,
        [FromQuery] Guid? companyId,
        [FromQuery] string? jobType,
        [FromQuery] string? experienceLevel,
        [FromQuery] string? city,
        [FromQuery] bool? isRemote,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] string? currency,
        [FromQuery] List<string>? skills,
        [FromQuery] List<string>? tags,
        [FromQuery] DateTime? postedAfter,
        [FromQuery] DateTime? postedBefore,
        [FromQuery] string sortBy = "Relevance",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchJobPostingsQuery(
            Keywords: keywords,
            CompanyId: companyId,
            JobType: null, // TODO: Parse from string
            ExperienceLevel: null, // TODO: Parse from string
            Status: null,
            City: city,
            IsRemote: isRemote,
            MinSalary: minSalary,
            MaxSalary: maxSalary,
            Currency: currency,
            RequiredSkills: skills,
            Tags: tags,
            PostedAfter: postedAfter,
            PostedBefore: postedBefore,
            SortBy: Enum.TryParse<SearchSortBy>(sortBy, true, out var sortByEnum) ? sortByEnum : SearchSortBy.Relevance,
            SortDescending: true,
            Page: page,
            PageSize: pageSize);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get job posting by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<JobPostingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetJobPostingByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Create a new job posting
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<JobPostingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.JobPostingId },
            ApiResponses.Success(result.Value, "Job posting created successfully"));
    }

    /// <summary>
    /// Update an existing job posting
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<JobPostingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.JobPostingId)
        {
            return BadRequest(ApiResponses.Failure("ID mismatch"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Publish a job posting
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishJobPostingCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Job posting published successfully"));
    }

    /// <summary>
    /// Close a job posting
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Close(
        Guid id,
        [FromBody] CloseJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.JobPostingId)
        {
            return BadRequest(ApiResponses.Failure("ID mismatch"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Job posting closed successfully"));
    }

    /// <summary>
    /// Save a job posting to user's favorites
    /// </summary>
    [HttpPost("{id:guid}/save")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveJob(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new SaveJobCommand(userId, id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Job saved successfully"));
    }

    /// <summary>
    /// Remove a job posting from user's favorites
    /// </summary>
    [HttpDelete("{id:guid}/save")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnsaveJob(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new UnsaveJobCommand(userId, id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Job unsaved successfully"));
    }

    /// <summary>
    /// Get user's saved jobs
    /// </summary>
    [HttpGet("saved")]
    [ProducesResponseType(typeof(ApiResponse<SavedJobsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavedJobs(
        [FromQuery] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSavedJobsQuery(userId, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Check if a job is saved by user
    /// </summary>
    [HttpGet("{id:guid}/saved")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsJobSaved(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var query = new IsJobSavedQuery(userId, id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success((object)new { isSaved = result.Value }));
    }
}
