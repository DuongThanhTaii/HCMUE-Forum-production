using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Career.Application.Commands.Recruiters.AddRecruiter;
using UniHub.Career.Application.Commands.Recruiters.DeactivateRecruiter;
using UniHub.Career.Application.Commands.Recruiters.ReactivateRecruiter;
using UniHub.Career.Application.Commands.Recruiters.UpdatePermissions;
using UniHub.Career.Application.Queries.Recruiters.GetRecruitersForCompany;
using UniHub.Career.Application.Queries.Recruiters.IsUserRecruiter;

namespace UniHub.Career.Presentation.Controllers;

/// <summary>
/// Controller for managing recruiters and their permissions
/// </summary>
[ApiController]
[Route("api/v1/recruiters")]
[Produces("application/json")]
[Authorize]
public class RecruitersController : ControllerBase
{
    private readonly ISender _sender;

    public RecruitersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Add a new recruiter to a company
    /// </summary>
    /// <param name="command">Recruiter details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created recruiter</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RecruiterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddRecruiter(
        [FromBody] AddRecruiterCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetRecruitersForCompany),
            new { companyId = result.Value.CompanyId },
            ApiResponses.Success(result.Value, "Recruiter added successfully"));
    }

    /// <summary>
    /// Get all recruiters for a specific company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="activeOnly">Filter for active recruiters only</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recruiters</returns>
    [HttpGet("companies/{companyId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RecruitersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecruitersForCompany(
        Guid companyId,
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecruitersForCompanyQuery(
            CompanyId: companyId,
            ActiveOnly: activeOnly);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Check if a user is a recruiter for a specific company
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <param name="companyId">Company ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if user is a recruiter</returns>
    [HttpGet("check")]
    [ProducesResponseType(typeof(ApiResponse<IsRecruiterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIsRecruiter(
        [FromQuery] Guid userId,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var query = new IsUserRecruiterQuery(userId, companyId);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Update recruiter permissions
    /// </summary>
    /// <param name="id">Recruiter ID</param>
    /// <param name="command">Updated permissions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPut("{id:guid}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermissions(
        Guid id,
        [FromBody] UpdateRecruiterPermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RecruiterId)
        {
            return BadRequest(ApiResponses.Failure("Recruiter ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Recruiter permissions updated successfully"));
    }

    /// <summary>
    /// Deactivate a recruiter
    /// </summary>
    /// <param name="id">Recruiter ID</param>
    /// <param name="command">Deactivation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRecruiter(
        Guid id,
        [FromBody] DeactivateRecruiterCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RecruiterId)
        {
            return BadRequest(ApiResponses.Failure("Recruiter ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Recruiter deactivated successfully"));
    }

    /// <summary>
    /// Reactivate a previously deactivated recruiter
    /// </summary>
    /// <param name="id">Recruiter ID</param>
    /// <param name="command">Reactivation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateRecruiter(
        Guid id,
        [FromBody] ReactivateRecruiterCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.RecruiterId)
        {
            return BadRequest(ApiResponses.Failure("Recruiter ID in route does not match the one in request body"));
        }

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Recruiter reactivated successfully"));
    }
}
