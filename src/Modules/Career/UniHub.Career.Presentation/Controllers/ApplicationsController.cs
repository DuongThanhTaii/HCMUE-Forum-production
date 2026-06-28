using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Career.Application.Commands.Applications.AcceptApplication;
using UniHub.Career.Application.Commands.Applications.RejectApplication;
using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.Career.Application.Commands.Applications.UpdateApplicationStatus;
using UniHub.Career.Application.Commands.Applications.WithdrawApplication;
using UniHub.Career.Application.Queries.Applications.GetApplicationById;
using UniHub.Career.Application.Queries.Applications.GetApplicationsByApplicant;
using UniHub.Career.Application.Queries.Applications.GetApplicationsByJob;
using UniHub.Contracts;

namespace UniHub.Career.Presentation.Controllers;

/// <summary>
/// Controller for managing job applications
/// </summary>
[Route("api/v1/applications")]
[Produces("application/json")]
[Authorize]
public class ApplicationsController : BaseApiController
{
    private readonly ISender _sender;

    public ApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Submit a new job application
    /// </summary>
    /// <param name="command">Application submission details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created application</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ApplicationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitApplication(
        [FromBody] SubmitApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetApplicationById),
            new { id = result.Value.Id },
            ApiResponses.Success(result.Value, "Application submitted successfully"));
    }

    /// <summary>
    /// Get all applications for the current user (as applicant)
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's applications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ApplicationListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationsByApplicantQuery(
            ApplicantId: GetCurrentUserId(),
            Status: null,
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
    /// Get a specific application by ID
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Application details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetApplicationById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationByIdQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Update the status of an application (recruiter action)
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="command">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApplicationStatus(
        Guid id,
        [FromBody] UpdateApplicationStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ApplicationId)
        {
            return BadRequest(ApiResponses.Failure("Application ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Application status updated successfully"));
    }

    /// <summary>
    /// Withdraw an application (applicant action)
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="command">Withdrawal details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> WithdrawApplication(
        Guid id,
        [FromBody] WithdrawApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ApplicationId)
        {
            return BadRequest(ApiResponses.Failure("Application ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Application withdrawn successfully"));
    }

    /// <summary>
    /// Accept a job offer (applicant action)
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="command">Acceptance details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptApplication(
        Guid id,
        [FromBody] AcceptApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ApplicationId)
        {
            return BadRequest(ApiResponses.Failure("Application ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Job offer accepted successfully"));
    }

    /// <summary>
    /// Reject an application (recruiter action)
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="command">Rejection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectApplication(
        Guid id,
        [FromBody] RejectApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.ApplicationId)
        {
            return BadRequest(ApiResponses.Failure("Application ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Application rejected successfully"));
    }

    /// <summary>
    /// Get all applications for a specific job posting (recruiter action)
    /// </summary>
    /// <param name="jobId">Job posting ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of applications for the job</returns>
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ApplicationListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetApplicationsByJob(
        Guid jobId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationsByJobQuery(
            JobPostingId: jobId,
            Status: null,
            Page: page,
            PageSize: pageSize);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }
}
