using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.RemoveModerator;

/// <summary>
/// Handler for removing a moderator from a channel
/// </summary>
public sealed class RemoveModeratorCommandHandler : ICommandHandler<RemoveModeratorCommand>
{
    private readonly IChannelRepository _channelRepository;

    public RemoveModeratorCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(
        RemoveModeratorCommand request,
        CancellationToken cancellationToken)
    {
        // Get channel
        var channelId = ChannelId.Create(request.ChannelId);
        var channel = await _channelRepository.GetByIdAsync(channelId, cancellationToken);

        if (channel is null)
        {
            return Result.Failure(new Error(
                "Channel.NotFound",
                $"Channel with ID {request.ChannelId} not found"));
        }

        // Remove moderator using domain method
        var result = channel.RemoveModerator(request.UserId, request.RemovedBy);

        if (result.IsFailure)
        {
            return result;
        }

        // Update channel
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return Result.Success();
    }
}
