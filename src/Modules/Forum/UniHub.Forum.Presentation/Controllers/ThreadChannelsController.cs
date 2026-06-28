using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.ThreadChannels;
using UniHub.Forum.Presentation.DTOs.ThreadChannels;
using UniHub.SharedKernel.Persistence;

namespace UniHub.Forum.Presentation.Controllers;

[Route("api/v1/thread-channels")]
[Produces("application/json")]
public sealed class ThreadChannelsController : BaseApiController
{
    private readonly IThreadChannelRepository _threadChannelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ThreadChannelsController(
        IThreadChannelRepository threadChannelRepository,
        IUnitOfWork unitOfWork)
    {
        _threadChannelRepository = threadChannelRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveChannels(CancellationToken cancellationToken = default)
    {
        var channels = await _threadChannelRepository.GetAllAsync(includeInactive: false, cancellationToken);
        return Ok(ApiResponses.Success(new
        {
            channels = channels.Select(MapResponse).ToList()
        }));
    }

    [HttpGet("admin")]
    [RequirePermission("forum.thread_channels.manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllChannelsForAdmin(CancellationToken cancellationToken = default)
    {
        var channels = await _threadChannelRepository.GetAllAsync(includeInactive: true, cancellationToken);
        return Ok(ApiResponses.Success(new
        {
            channels = channels.Select(MapResponse).ToList()
        }));
    }

    [HttpPost]
    [RequirePermission("forum.thread_channels.manage")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChannel(
        [FromBody] ThreadChannelUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validate = ValidateRequest(request);
        if (validate is not null)
        {
            return BadRequest(ApiResponses.Failure(validate));
        }

        if (await _threadChannelRepository.ExistsByCodeAsync(request.Code, null, cancellationToken))
        {
            return BadRequest(ApiResponses.Failure("Thread channel code already exists."));
        }

        var channel = ThreadChannel.Create(
            request.Code,
            request.Name,
            request.Description,
            request.DisplayOrder,
            request.IsActive,
            request.AllowPinnedComments,
            request.AllowAcceptedAnswers,
            request.AllowModeratorActions);

        await _threadChannelRepository.AddAsync(channel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Created(string.Empty, ApiResponses.Success(MapResponse(channel), "Thread channel created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("forum.thread_channels.manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChannel(
        Guid id,
        [FromBody] ThreadChannelUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validate = ValidateRequest(request);
        if (validate is not null)
        {
            return BadRequest(ApiResponses.Failure(validate));
        }

        var channel = await _threadChannelRepository.GetByIdAsync(id, cancellationToken);
        if (channel is null)
        {
            return NotFound(ApiResponses.Failure("Thread channel not found."));
        }

        if (await _threadChannelRepository.ExistsByCodeAsync(request.Code, id, cancellationToken))
        {
            return BadRequest(ApiResponses.Failure("Thread channel code already exists."));
        }

        channel.Update(
            request.Code,
            request.Name,
            request.Description,
            request.DisplayOrder,
            request.IsActive,
            request.AllowPinnedComments,
            request.AllowAcceptedAnswers,
            request.AllowModeratorActions);

        await _threadChannelRepository.UpdateAsync(channel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponses.Success(MapResponse(channel), "Thread channel updated successfully."));
    }

    private static string? ValidateRequest(ThreadChannelUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return "Code is required.";
        }

        if (request.Code.Length > 64)
        {
            return "Code must be at most 64 characters.";
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Name is required.";
        }

        if (request.Name.Length > 120)
        {
            return "Name must be at most 120 characters.";
        }

        return null;
    }

    private static ThreadChannelResponse MapResponse(ThreadChannel channel)
    {
        return new ThreadChannelResponse
        {
            Id = channel.Id,
            Code = channel.Code,
            Name = channel.Name,
            Description = channel.Description,
            DisplayOrder = channel.DisplayOrder,
            IsActive = channel.IsActive,
            AllowPinnedComments = channel.AllowPinnedComments,
            AllowAcceptedAnswers = channel.AllowAcceptedAnswers,
            AllowModeratorActions = channel.AllowModeratorActions
        };
    }
}
