using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Learning.Application.Commands.FacultyManagement.CreateFaculty;
using UniHub.Learning.Application.Queries.Faculties.GetFaculties;
using UniHub.Learning.Application.Queries.Faculties.GetFacultyById;
using UniHub.Learning.Presentation.DTOs.Faculties;

namespace UniHub.Learning.Presentation.Controllers;

[ApiController]
[Route("api/v1/faculties")]
[Produces("application/json")]
public class FacultiesController : BaseApiController
{
    private readonly ISender _sender;

    public FacultiesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all faculties
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<FacultyListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFaculties(CancellationToken cancellationToken)
    {
        var query = new GetFacultiesQuery();
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get a faculty by its ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FacultyDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFacultyById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetFacultyByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Create a new faculty
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreateFacultyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFaculty(
        [FromBody] CreateFacultyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new CreateFacultyCommand(
            request.Code,
            request.Name,
            request.Description,
            userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new CreateFacultyResponse(result.Value, request.Code, request.Name);

        return CreatedAtAction(
            nameof(GetFaculties),
            new { id = response.FacultyId },
            ApiResponses.Success(response, "Faculty created successfully"));
    }
}
