using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Learning.Application.Commands.CourseManagement;
using UniHub.Learning.Application.Commands.ModeratorAssignment;
using UniHub.Learning.Application.Queries.Courses.GetCourseById;
using UniHub.Learning.Application.Queries.Courses.GetCourseSemesters;
using UniHub.Learning.Application.Queries.Courses.GetCourses;
using UniHub.Learning.Presentation.DTOs.Courses;

namespace UniHub.Learning.Presentation.Controllers;

[ApiController]
[Route("api/v1/courses")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// List courses with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedCourseListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] Guid? facultyId = null,
        [FromQuery] string? semester = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCoursesQuery(facultyId, semester, searchTerm, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Distinct semester values for filter dropdown (optional faculty scope).
    /// </summary>
    [HttpGet("semesters")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourseSemesters(
        [FromQuery] Guid? facultyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCourseSemestersQuery(facultyId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Get course details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(result.Value));
    }

    /// <summary>
    /// Create a new course
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreateCourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourseCommand(
            request.Code,
            request.Name,
            request.Description ?? string.Empty,
            request.Semester,
            request.Credits,
            request.CreatedBy,
            request.FacultyId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new CreateCourseResponse(result.Value, request.Code, request.Name);

        return CreatedAtAction(nameof(CreateCourse), new { id = response.CourseId }, ApiResponses.Success(response, "Course created successfully"));
    }

    /// <summary>
    /// Update course information
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCourse(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseCommand(
            id,
            request.Name,
            request.Description ?? string.Empty,
            request.Semester,
            request.Credits);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Course updated successfully"));
    }

    /// <summary>
    /// Delete a course (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCourse(
        Guid id,
        [FromBody] DeleteCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand(id, request.DeletedBy);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Course deleted successfully"));
    }

    /// <summary>
    /// Assign a moderator to a course
    /// </summary>
    [HttpPost("{id}/moderators")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignModerator(
        Guid id,
        [FromBody] AssignModeratorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignCourseModeratorCommand(id, request.ModeratorId, request.AssignedBy);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Moderator assigned successfully"));
    }

    /// <summary>
    /// Remove a moderator from a course
    /// </summary>
    [HttpDelete("{id}/moderators/{moderatorId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveModerator(
        Guid id,
        Guid moderatorId,
        [FromBody] RemoveModeratorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCourseModeratorCommand(id, moderatorId, request.RemovedBy);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Moderator removed successfully"));
    }
}
