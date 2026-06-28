using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.LeaveChannel;

/// <summary>
/// Handler for leaving a channel
/// </summary>
public sealed class LeaveChannelCommandHandler : ICommandHandler<LeaveChannelCommand>
{
    private readonly IChannelRepository _channelRepository;

    public LeaveChannelCommandHandler(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(
        LeaveChannelCommand request,
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

        // Leave channel using domain method
        var result = channel.Leave(request.UserId);

        if (result.IsFailure)
        {
            return result;
        }

        // Update channel
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return Result.Success();
    }
}
