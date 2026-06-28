using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniHub.Contracts;
using UniHub.Forum.Application.Commands.AcceptAnswer;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Application.Commands.DeleteComment;
using UniHub.Forum.Application.Commands.PinComment;
using UniHub.Forum.Application.Commands.ReportComment;
using UniHub.Forum.Application.Commands.UpdateComment;
using UniHub.Forum.Application.Commands.VoteComment;
using UniHub.Forum.Domain.Votes;
using UniHub.Forum.Presentation.DTOs.Comments;
using UniHub.Forum.Presentation.DTOs.Reports;
using UniHub.Forum.Presentation.DTOs.Votes;

namespace UniHub.Forum.Presentation.Controllers;

[Route("api/v1/comments")]
[Produces("application/json")]
public class CommentsController : BaseApiController
{
    private readonly ISender _sender;

    public CommentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
    [HttpPost("posts/{postId:guid}")]
    [Authorize]
    [EnableRateLimiting("forum-write")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(
        Guid postId,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new AddCommentCommand(
            postId,
            userId,
            request.Content,
            request.ParentCommentId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Created(string.Empty, ApiResponses.Success((object)new { commentId = result.Value }, "Comment created successfully"));
    }

    /// <summary>
    /// Update a comment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(
        Guid id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new UpdateCommentCommand(id, request.Content, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Comment updated successfully"));
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new DeleteCommentCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Comment deleted successfully"));
    }

    /// <summary>
    /// Vote on a comment (upvote or downvote)
    /// </summary>
    [HttpPost("{id:guid}/vote")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoteComment(
        Guid id,
        [FromBody] VoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var voteType = request.VoteType == 1 ? VoteType.Upvote : VoteType.Downvote;
        var command = new VoteCommentCommand(id, userId, voteType);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Comment voted successfully"));
    }

    /// <summary>
    /// Accept a comment as the answer to a question post
    /// </summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptAnswer(
        Guid id,
        [FromQuery] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new AcceptAnswerCommand(id, postId, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Answer accepted successfully"));
    }

    /// <summary>
    /// Toggle pin state for a comment inside a post thread
    /// </summary>
    [HttpPost("{id:guid}/pin")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TogglePinComment(
        Guid id,
        [FromQuery] Guid postId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var hasModerationPrivilege = User.IsInRole("Admin") || User.IsInRole("Moderator");

        var command = new PinCommentCommand(id, postId, userId, hasModerationPrivilege);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Comment pin state updated successfully"));
    }

    /// <summary>
    /// Report a comment
    /// </summary>
    [HttpPost("{id:guid}/report")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportComment(
        Guid id,
        [FromBody] ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new ReportCommentCommand(
            id,
            userId,
            (UniHub.Forum.Domain.Reports.ReportReason)request.Reason,
            request.Description);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Created(string.Empty, ApiResponses.Success((object)new { reportId = result.Value }, "Comment reported successfully"));
    }
}
